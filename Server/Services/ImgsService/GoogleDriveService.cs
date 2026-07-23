using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using File = Google.Apis.Drive.v3.Data.File;

namespace AquaSolution.Server.Services.ImgsService
{
    public class GoogleDriveService : IGoogleDriveService
    {
        private readonly IConfiguration _configuration;

        public GoogleDriveService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private DriveService GetDriveService()
        {
            var clientId = _configuration["GoogleDrive:ClientId"];
            var clientSecret = _configuration["GoogleDrive:ClientSecret"];
            var refreshToken = _configuration["GoogleDrive:RefreshToken"];

            var tokenResponse = new TokenResponse
            {
                RefreshToken = refreshToken
            };

            var credential = new UserCredential(new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = clientId,
                        ClientSecret = clientSecret
                    }
                }), "user", tokenResponse);

            return new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Image Uploader Web API"
            });
        }

        public async Task<(string url, string fileId)> UploadAsync(IFormFile file, string workDayId)
        {
            var folderId = _configuration["GoogleDrive:FolderId"];
            var service = GetDriveService();

            var targetFolderId = await GetOrCreateFolderAsync(service, folderId, workDayId);

            var fileMetadata = new File()
            {
                Name = $"{workDayId}_{Guid.NewGuid():N}_{file.FileName}",
                Parents = new List<string> { targetFolderId }
            };

            string fileId = null;

            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                var uploadStream = new MemoryStream(ms.ToArray());
                
                var request = service.Files.Create(fileMetadata, uploadStream, file.ContentType);
                request.Fields = "id";
                
                var response = await request.UploadAsync();
                
                if (response.Status != Google.Apis.Upload.UploadStatus.Completed)
                {
                    var errorMsg = response.Exception?.Message ?? "Unknown error";
                    throw new Exception($"Lỗi upload lên Google Drive. Chi tiết: {errorMsg}");
                }
                
                fileId = request.ResponseBody.Id;
            }

            // Cấp quyền Public
            var permission = new Google.Apis.Drive.v3.Data.Permission
            {
                Type = "anyone",
                Role = "reader"
            };
            await service.Permissions.Create(permission, fileId).ExecuteAsync();

            var link = $"https://drive.google.com/uc?id={fileId}";
            return (link, fileId);
        }

        public async Task DeleteIfExistsAsync(string fileId)
        {
            var service = GetDriveService();
            try
            {
                await service.Files.Delete(fileId).ExecuteAsync();
            }
            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Bỏ qua nếu file đã bị xóa
            }
        }

        private async Task<string> GetOrCreateFolderAsync(DriveService service, string baseFolderId, string folderName)
        {
            var safeFolderName = folderName.Replace("'", "\\'");
            var query = $"mimeType='application/vnd.google-apps.folder' and '{baseFolderId}' in parents and name='{safeFolderName}' and trashed=false";

            var listRequest = service.Files.List();
            listRequest.Q = query;
            listRequest.Fields = "files(id)";

            var listResponse = await listRequest.ExecuteAsync();

            if (listResponse.Files != null && listResponse.Files.Count > 0)
            {
                return listResponse.Files[0].Id;
            }

            var folderMetadata = new File()
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder",
                Parents = new List<string> { baseFolderId }
            };

            var createRequest = service.Files.Create(folderMetadata);
            createRequest.Fields = "id";

            var folder = await createRequest.ExecuteAsync();
            return folder.Id;
        }
    }
}
