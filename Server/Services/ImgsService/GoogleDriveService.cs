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

        public async Task DeleteOldFilesAsync(int daysOld)
        {
            var service = GetDriveService();
            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld).ToString("yyyy-MM-ddTHH:mm:ssK");
            
            // Lấy ID thư mục từ cấu hình
            var folderId = _configuration["GoogleDrive:FolderId"];
            
            if (string.IsNullOrEmpty(folderId))
            {
                // Fallback nếu không có cấu hình
                folderId = "1_hXVvUGEnyyj6WdbvVr_tX5oUSud48mZ";
            }

            // Gọi hàm đệ quy để xóa file trong thư mục hiện tại và các thư mục con
            await DeleteOldFilesInFolderRecursiveAsync(service, folderId, cutoffDate);
        }

        private async Task DeleteOldFilesInFolderRecursiveAsync(DriveService service, string currentFolderId, string cutoffDate)
        {
            // 1. Xóa tất cả các file (không phải folder) trong thư mục hiện tại
            var fileRequest = service.Files.List();
            fileRequest.Q = $"'{currentFolderId}' in parents and mimeType != 'application/vnd.google-apps.folder' and createdTime < '{cutoffDate}' and trashed=false";
            fileRequest.Fields = "nextPageToken, files(id, name, createdTime)";
            fileRequest.PageSize = 100;

            do
            {
                var result = await fileRequest.ExecuteAsync();
                if (result.Files != null)
                {
                    foreach (var file in result.Files)
                    {
                        try
                        {
                            await service.Files.Delete(file.Id).ExecuteAsync();
                            Console.WriteLine($"Deleted old file: {file.Name} ({file.Id}) created at {file.CreatedTime}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to delete file {file.Id}: {ex.Message}");
                        }
                    }
                }
                fileRequest.PageToken = result.NextPageToken;
            } while (fileRequest.PageToken != null);

            // 2. Tìm tất cả các thư mục con và gọi đệ quy
            var folderRequest = service.Files.List();
            folderRequest.Q = $"'{currentFolderId}' in parents and mimeType = 'application/vnd.google-apps.folder' and trashed=false";
            folderRequest.Fields = "nextPageToken, files(id, name)";
            folderRequest.PageSize = 100;

            do
            {
                var result = await folderRequest.ExecuteAsync();
                if (result.Files != null)
                {
                    foreach (var folder in result.Files)
                    {
                        // Đệ quy vào từng thư mục con
                        await DeleteOldFilesInFolderRecursiveAsync(service, folder.Id, cutoffDate);
                    }
                }
                folderRequest.PageToken = result.NextPageToken;
            } while (folderRequest.PageToken != null);
        }
    }
}
