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
            
            // Tìm tất cả các file (không phải folder) cũ hơn số ngày quy định
            var request = service.Files.List();
            request.Q = $"mimeType != 'application/vnd.google-apps.folder' and createdTime < '{cutoffDate}' and trashed=false";
            request.Fields = "nextPageToken, files(id, name, createdTime)";
            request.PageSize = 100;

            do
            {
                var result = await request.ExecuteAsync();
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
                request.PageToken = result.NextPageToken;
            } while (request.PageToken != null);
        }
    }
}
