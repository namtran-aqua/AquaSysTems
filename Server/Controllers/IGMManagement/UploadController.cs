using Microsoft.AspNetCore.Mvc;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using File = Google.Apis.Drive.v3.Data.File;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace AquaSolution.Server.Controllers.IGMManagement
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // Uncomment this if you want to require authentication
    public class UploadController : ControllerBase
    {
        // TODO: Sửa lại đường dẫn tới file JSON và ID thư mục của bạn
        private readonly string _credentialsFilePath = "google-credentials.json"; 
        private readonly string _folderId = "YOUR_GOOGLE_DRIVE_FOLDER_ID"; // Điền ID thư mục ở Bước 1 vào đây

        [HttpPost]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file, [FromForm] string workId)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Không có file tải lên." });

            try
            {
                // 1. Khởi tạo xác thực với Google Service Account
                GoogleCredential credential;
                using (var stream = new FileStream(_credentialsFilePath, FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream)
                        .CreateScoped(DriveService.ScopeConstants.DriveFile);
                }

                // 2. Tạo Service kết nối Drive
                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Image Uploader Web API"
                });

                // 3. Chuẩn bị Metadata (Tên file và thư mục lưu)
                // Đặt tên file theo định dạng: WorkId_Guid_OriginalName
                var fileMetadata = new File()
                {
                    Name = $"{workId}_{Guid.NewGuid():N}_{file.FileName}",
                    Parents = new List<string> { _folderId }
                };

                string fileId = null;

                // 4. Đọc luồng file gửi từ App và Upload lên Drive
                using (var ms = new MemoryStream())
                {
                    await file.CopyToAsync(ms);
                    var uploadStream = new MemoryStream(ms.ToArray());
                    
                    var request = service.Files.Create(fileMetadata, uploadStream, file.ContentType);
                    request.Fields = "id"; // Chỉ yêu cầu trả về ID sau khi upload xong
                    
                    var response = await request.UploadAsync();
                    
                    if (response.Status != Google.Apis.Upload.UploadStatus.Completed)
                    {
                        return StatusCode(500, new { message = "Lỗi upload lên Google Drive." });
                    }
                    
                    fileId = request.ResponseBody.Id;
                }

                // 5. Cấp quyền Public (Bất kỳ ai có link đều xem được)
                var permission = new Google.Apis.Drive.v3.Data.Permission
                {
                    Type = "anyone",
                    Role = "reader"
                };
                await service.Permissions.Create(permission, fileId).ExecuteAsync();

                // 6. Trả về kết quả cho Mobile App
                var link = $"https://drive.google.com/uc?id={fileId}";
                
                return Ok(new 
                { 
                    url = link,
                    fileId = fileId,
                    message = "Upload thành công!" 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }

        [HttpGet("workday-ids")]
        public IActionResult GetWorkDayIds()
        {
            // TODO: Fetch from database instead of returning static list
            var workDayIds = new[] { "Work001", "Work002", "Work003" };
            return Ok(workDayIds);
        }
    }
}
