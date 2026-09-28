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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using AquaSolution.Shared.Imgs;
using File = Google.Apis.Drive.v3.Data.File;

namespace AquaSolution.Server.Services.ImgsService
{
    public class GoogleDriveService : IGoogleDriveService
    {
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;

        public GoogleDriveService(IConfiguration configuration, IMemoryCache cache)
        {
            _configuration = configuration;
            _cache = cache;
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

        public async Task<Dictionary<string, string>> GetFoldersAsync()
        {
            var service = GetDriveService();
            var rootFolderId = _configuration["GoogleDrive:FolderId"] ?? "1_hXVvUGEnyyj6WdbvVr_tX5oUSud48mZ";
            
            var folderRequest = service.Files.List();
            folderRequest.Q = $"'{rootFolderId}' in parents and mimeType = 'application/vnd.google-apps.folder' and trashed=false";
            folderRequest.Fields = "nextPageToken, files(id, name)";
            folderRequest.PageSize = 1000;

            var folders = new Dictionary<string, string>();
            do
            {
                var folderRes = await folderRequest.ExecuteAsync();
                if (folderRes.Files != null)
                {
                    foreach (var folder in folderRes.Files)
                    {
                        folders[folder.Id] = folder.Name;
                    }
                }
                folderRequest.PageToken = folderRes.NextPageToken;
            } while (folderRequest.PageToken != null);

            return folders;
        }

        public async Task<List<GoogleDriveImageDto>> GetAllImagesAsync(List<string> allowedFolderNames)
        {
            const string cacheKey = "GoogleDrive_AllImages";
            
            if (!_cache.TryGetValue(cacheKey, out List<GoogleDriveImageDto> allImages))
            {
                allImages = await FetchAllImagesFromDriveAsync();
                
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(60));
                
                _cache.Set(cacheKey, allImages, cacheEntryOptions);
            }

            if (allowedFolderNames != null)
            {
                return allImages.Where(x => allowedFolderNames.Contains(x.FolderName)).ToList();
            }

            return allImages;
        }

        private async Task<List<GoogleDriveImageDto>> FetchAllImagesFromDriveAsync()
        {
            var service = GetDriveService();
            var rootFolderId = _configuration["GoogleDrive:FolderId"] ?? "1_hXVvUGEnyyj6WdbvVr_tX5oUSud48mZ";
            
            var result = new List<GoogleDriveImageDto>();
            
            var folderRequest = service.Files.List();
            folderRequest.Q = $"'{rootFolderId}' in parents and mimeType = 'application/vnd.google-apps.folder' and trashed=false";
            folderRequest.Fields = "nextPageToken, files(id, name)";
            folderRequest.PageSize = 1000;

            var folders = new Dictionary<string, string>();
            do
            {
                var folderRes = await folderRequest.ExecuteAsync();
                if (folderRes.Files != null)
                {
                    foreach (var folder in folderRes.Files)
                    {
                        folders[folder.Id] = folder.Name;
                    }
                }
                folderRequest.PageToken = folderRes.NextPageToken;
            } while (folderRequest.PageToken != null);

            if (!folders.Any()) return result;

            var tasks = folders.Select(async folder =>
            {
                var folderResult = new List<GoogleDriveImageDto>();
                var serviceInstance = GetDriveService(); // Each task might need its own service instance if not thread-safe, but usually it's fine.
                var fileRequest = serviceInstance.Files.List();
                fileRequest.Q = $"'{folder.Key}' in parents and trashed=false";
                fileRequest.Fields = "nextPageToken, files(id, name, mimeType, parents, size, createdTime, thumbnailLink)";
                fileRequest.PageSize = 1000;

                do
                {
                    var fileRes = await fileRequest.ExecuteAsync();
                    if (fileRes.Files != null)
                    {
                        foreach (var file in fileRes.Files)
                        {
                            if (file.MimeType != null && file.MimeType.StartsWith("image/"))
                            {
                                folderResult.Add(new GoogleDriveImageDto
                                {
                                    FileId = file.Id,
                                    FileName = file.Name,
                                    FolderId = folder.Key,
                                    FolderName = folder.Value,
                                    FileSize = file.Size,
                                    CreatedTime = file.CreatedTimeDateTimeOffset?.AddHours(7).DateTime,
                                    ThumbnailLink = file.ThumbnailLink
                                });
                            }
                        }
                    }
                    fileRequest.PageToken = fileRes.NextPageToken;
                } while (fileRequest.PageToken != null);

                return folderResult;
            });

            var results = await Task.WhenAll(tasks);
            foreach (var res in results)
            {
                result.AddRange(res);
            }

            return result;
        }

        public async Task<byte[]> GetFileBytesAsync(string fileId)
        {
            var service = GetDriveService();
            var request = service.Files.Get(fileId);
            using var memoryStream = new MemoryStream();
            await request.DownloadAsync(memoryStream);
            return memoryStream.ToArray();
        }

        public async Task<bool> DeleteFileAsync(string fileId)
        {
            try
            {
                var service = GetDriveService();
                await service.Files.Delete(fileId).ExecuteAsync();
                _cache.Remove("GoogleDrive_AllImages");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
