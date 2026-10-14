using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using AquaSolution.Server.Common;
using AquaSolution.Server.Services.Feedbacks;
using AquaSolution.Shared.Feedbacks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AquaSolution.Server.Controllers.Feedbacks
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserFeedbackController : ControllerBase
    {
        private readonly IUserFeedbackService _feedbackService;
        private readonly IFeedbackSyncService _syncService;
        private readonly AquaSolution.Server.Services.ImgsService.IGoogleDriveService _driveService;
        private readonly AquaSolution.Data.Connection.AquaDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly Microsoft.Extensions.Logging.ILogger<UserFeedbackController> _logger;

        public UserFeedbackController(
            IUserFeedbackService feedbackService,
            IFeedbackSyncService syncService,
            AquaSolution.Server.Services.ImgsService.IGoogleDriveService driveService,
            AquaSolution.Data.Connection.AquaDbContext context,
            IWebHostEnvironment env,
            Microsoft.Extensions.Logging.ILogger<UserFeedbackController> logger)
        {
            _feedbackService = feedbackService;
            _syncService = syncService;
            _driveService = driveService;
            _context = context;
            _env = env;
            _logger = logger;
        }

        /// <summary>
        /// Public endpoint to get list of departments for dropdown
        /// </summary>
        [AllowAnonymous]
        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            var depts = await _feedbackService.GetDepartmentListAsync();
            return Ok(depts);
        }

        /// <summary>
        /// Public endpoint to get list of factories for dropdown
        /// </summary>
        [AllowAnonymous]
        [HttpGet("factories")]
        public async Task<IActionResult> GetFactories()
        {
            var factories = await _feedbackService.GetFactoryListAsync();
            return Ok(factories);
        }

        /// <summary>
        /// Public endpoint to submit feedback with multiple files (No login required)
        /// </summary>
        [AllowAnonymous]
        [HttpPost("submit")]
        [RequestSizeLimit(262144000)] // 250MB max total request size
        [RequestFormLimits(MultipartBodyLengthLimit = 262144000)]
        public async Task<IActionResult> SubmitFeedback(
            [FromForm] string fullName,
            [FromForm] int? age,
            [FromForm] string? factory,
            [FromForm] string department,
            [FromForm] string? phoneNumber,
            [FromForm] string title,
            [FromForm] string content,
            [FromForm] List<IFormFile>? files)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return BadRequest("Họ và tên không được để trống.");

            if (string.IsNullOrWhiteSpace(department))
                return BadRequest("Phòng ban không được để trống.");

            if (string.IsNullOrWhiteSpace(title))
                return BadRequest("Tiêu đề không được để trống.");

            if (string.IsNullOrWhiteSpace(content))
                return BadRequest("Nội dung không được để trống.");

            var attachments = new List<UserFeedbackAttachmentCreateDto>();

            if (files != null && files.Count > 0)
            {
                var uploadDir = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "feedbacks");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                foreach (var file in files)
                {
                    if (file.Length == 0) continue;

                    var originalName = Path.GetFileName(file.FileName);
                    var extension = Path.GetExtension(file.FileName);
                    var uniqueFileName = $"{Guid.NewGuid():N}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                    var physicalPath = Path.Combine(uploadDir, uniqueFileName);

                    using (var stream = new FileStream(physicalPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var relativeUrl = $"/uploads/feedbacks/{uniqueFileName}";

                    attachments.Add(new UserFeedbackAttachmentCreateDto
                    {
                        FileName = uniqueFileName,
                        OriginalFileName = originalName,
                        FilePath = relativeUrl,
                        FileType = extension,
                        FileSize = file.Length
                    });
                }
            }

            var createDto = new UserFeedbackCreateDto
            {
                FullName = fullName,
                Age = age,
                Factory = factory ?? string.Empty,
                Department = department,
                PhoneNumber = phoneNumber,
                Title = title,
                Content = content,
                Attachments = attachments
            };

            var result = await _feedbackService.SubmitFeedbackAsync(createDto);
            return Ok(new { success = true, data = result, message = "Gửi ý kiến thành công!" });
        }

        /// <summary>
        /// Admin endpoint: Get paged feedbacks with filters
        /// </summary>
        [HttpGet("list")]
        public async Task<IActionResult> GetPaged([FromQuery] UserFeedbackFilterDto filter)
        {
            var result = await _feedbackService.GetPagedAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Admin endpoint: Get feedback by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _feedbackService.GetByIdAsync(id);
            if (result == null) return NotFound("Không tìm thấy ý kiến.");
            return Ok(result);
        }

        /// <summary>
        /// Admin endpoint: Get dashboard stats
        /// </summary>
        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var result = await _feedbackService.GetDashboardStatsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Admin endpoint: Update status and admin note
        /// </summary>
        [HttpPut("update-status")]
        public async Task<IActionResult> UpdateStatus([FromBody] UserFeedbackStatusUpdateDto dto)
        {
            var currentUserName = User.FindFirst(ClaimTypes.Name)?.Value 
                                  ?? User.FindFirst("name")?.Value 
                                  ?? User.Identity?.Name 
                                  ?? "Admin";

            var success = await _feedbackService.UpdateStatusAsync(dto, currentUserName);
            if (!success) return BadRequest("Cập nhật trạng thái thất bại.");
            return Ok(new { success = true, message = "Cập nhật trạng thái thành công." });
        }

        /// <summary>
        /// Admin endpoint: Delete feedback
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _feedbackService.DeleteAsync(id);
            if (!success) return NotFound("Không tìm thấy ý kiến cần xóa.");
            return Ok(new { success = true, message = "Xóa ý kiến thành công." });
        }

        /// <summary>
        /// Trigger sync from Google Sheet
        /// </summary>
        [HttpPost("sync")]
        public async Task<IActionResult> SyncFromGoogleSheet()
        {
            try
            {
                var result = await _syncService.SyncAsync("Manual");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get latest sync status
        /// </summary>
        [HttpGet("sync-status")]
        public async Task<IActionResult> GetSyncStatus()
        {
            var status = await _syncService.GetSyncStatusAsync();
            return Ok(status);
        }

        /// <summary>
        /// Proxy download/stream attachment via GoogleDriveService (Secure & no public drive link needed)
        /// </summary>
        [HttpGet("attachments/{id}")]
        public async Task<IActionResult> GetAttachment(Guid id)
        {
            var att = await _context.tbl_UserFeedbackAttachments.FindAsync(id);
            if (att == null) return NotFound("Không tìm thấy tệp đính kèm.");

            try
            {
                // ƯU TIÊN 1: Phục vụ trực tiếp từ tệp cục bộ trên máy chủ (wwwroot/uploads/feedbacks)
                // Đảm bảo tốc độ cao nhất và hoạt động vĩnh viễn kể cả khi Drive đã xóa file sau 30 ngày
                if (!string.IsNullOrWhiteSpace(att.FilePath) && !att.FilePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    var cleanPath = att.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                    var localPhysicalPath = Path.Combine(_env.WebRootPath ?? "wwwroot", cleanPath);
                    if (System.IO.File.Exists(localPhysicalPath))
                    {
                        var bytes = await System.IO.File.ReadAllBytesAsync(localPhysicalPath);
                        var contentType = !string.IsNullOrWhiteSpace(att.FileType) ? att.FileType : "application/octet-stream";
                        Response.Headers["X-Content-Type-Options"] = "nosniff";
                        Response.Headers["Content-Disposition"] = $"inline; filename=\"{Uri.EscapeDataString(att.OriginalFileName ?? "file")}\"";
                        return File(bytes, contentType);
                    }
                }

                // ƯU TIÊN 2: Nếu chưa có file cục bộ trên máy chủ, tải từ Google Drive về, lưu bản sao vào server disk rồi phục vụ
                if (!string.IsNullOrWhiteSpace(att.GoogleDriveFileId))
                {
                    var fileBytes = await _driveService.GetFileBytesAsync(att.GoogleDriveFileId);
                    var fileName = att.OriginalFileName ?? $"{att.GoogleDriveFileId}.bin";

                    // Kiểm tra an toàn tệp: Magic Bytes, chống giả mạo đuôi, chống file thực thi
                    var (isValid, reason, detectedMime) = FileSecurityHelper.ValidateFile(fileBytes, fileName);
                    if (!isValid)
                    {
                        return BadRequest($"Tệp không an toàn: {reason}");
                    }

                    var contentType = detectedMime;

                    // Tự động lưu bản sao vĩnh viễn vào server disk
                    try
                    {
                        var uploadDir = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "feedbacks");
                        if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                        var ext = Path.GetExtension(fileName);
                        if (string.IsNullOrEmpty(ext)) ext = contentType == "application/pdf" ? ".pdf" : ".jpg";
                        var serverFileName = $"{att.GoogleDriveFileId}{ext}";
                        var localPhysicalPath = Path.Combine(uploadDir, serverFileName);
                        await System.IO.File.WriteAllBytesAsync(localPhysicalPath, fileBytes);

                        att.FilePath = $"/uploads/feedbacks/{serverFileName}";
                        att.FileType = contentType;
                        att.FileSize = fileBytes.Length;
                        att.SyncStatus = "LocalSaved";
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception saveEx)
                    {
                        _logger.LogWarning(saveEx, "Không thể lưu bản sao tệp {Id} vào disk máy chủ", id);
                    }

                    Response.Headers["X-Content-Type-Options"] = "nosniff";
                    Response.Headers["Content-Disposition"] = $"inline; filename=\"{Uri.EscapeDataString(fileName)}\"";
                    return File(fileBytes, contentType);
                }

                return NotFound("Tệp không tồn tại trên hệ thống lưu trữ.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi tải tệp: {ex.Message}");
            }
        }
    }
}
