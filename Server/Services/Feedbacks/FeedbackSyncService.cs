using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AquaSolution.Data.Connection;
using AquaSolution.Data.Data.Entities.Feedbacks;
using AquaSolution.Server.Services.ImgsService;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Feedbacks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using AquaSolution.Server.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AquaSolution.Server.Services.Feedbacks
{
    public class FeedbackSyncService : IFeedbackSyncService
    {
        private readonly AquaDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IGoogleDriveService _driveService;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FeedbackSyncService> _logger;

        // Khóa chống chạy đồng thời (No concurrent sync)
        private static readonly SemaphoreSlim _syncLock = new(1, 1);

        public FeedbackSyncService(
            AquaDbContext context,
            IConfiguration configuration,
            IGoogleDriveService driveService,
            IWebHostEnvironment env,
            ILogger<FeedbackSyncService> logger)
        {
            _context = context;
            _configuration = configuration;
            _driveService = driveService;
            _env = env;
            _logger = logger;
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
                ApplicationName = "Aqua Feedback Sync Service"
            });
        }

        public async Task<FeedbackSyncResultDto> SyncAsync(string triggerType = "Manual", CancellationToken cancellationToken = default)
        {
            // Kiểm tra và khóa tiến trình đồng bộ
            var acquired = await _syncLock.WaitAsync(TimeSpan.FromSeconds(3), cancellationToken);
            if (!acquired)
            {
                return new FeedbackSyncResultDto
                {
                    Success = false,
                    Message = "Một tiến trình đồng bộ đang chạy. Vui lòng thử lại sau vài giây."
                };
            }

            var syncLog = new UserFeedbackSyncLog
            {
                Id = Guid.NewGuid(),
                StartedAt = DateTime.Now,
                Status = "Running",
                TriggerType = triggerType
            };

            try
            {
                _context.tbl_UserFeedbackSyncLogs.Add(syncLog);
                await _context.SaveChangesAsync(cancellationToken);

                var spreadsheetId = _configuration["GoogleSheetFeedback:SpreadsheetId"] ?? "1TLT9XZgoWjyj_8Am0zQzponw9Rkk-QjkNQg08OLq6CQ";
                
                // 1. Tải CSV từ Google Drive Export
                var drive = GetDriveService();
                var exportReq = drive.Files.Export(spreadsheetId, "text/csv");
                using var memoryStream = new MemoryStream();
                await exportReq.DownloadAsync(memoryStream, cancellationToken);
                memoryStream.Position = 0;

                using var reader = new StreamReader(memoryStream, Encoding.UTF8);
                var csvContent = await reader.ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(csvContent))
                {
                    throw new Exception("Nội dung Google Sheet trống hoặc không thể tải về.");
                }

                // 2. Phân tích các dòng CSV
                var rows = ParseCsv(csvContent);
                if (rows.Count <= 1)
                {
                    syncLog.Status = "Success";
                    syncLog.CompletedAt = DateTime.Now;
                    syncLog.TotalRows = 0;
                    syncLog.NewRows = 0;
                    syncLog.MessageUpdate("Google Sheet chưa có dữ liệu phản ánh nào.");
                    await _context.SaveChangesAsync(cancellationToken);

                    return new FeedbackSyncResultDto
                    {
                        Success = true,
                        TotalRows = 0,
                        NewCount = 0,
                        Message = "Google Sheet chưa có dòng dữ liệu nào."
                    };
                }

                var headerRow = rows[0];
                var colMap = MapHeaders(headerRow);

                if (!colMap.ContainsKey("submissionid"))
                {
                    throw new Exception("Google Sheet thiếu cột bắt buộc 'SubmissionId'.");
                }

                // 3. Lấy danh sách SubmissionId đã có trong SQL để kiểm tra chống trùng lặp (Lớp 1 - Application)
                var existingIdList = await _context.tbl_UserFeedbacks
                    .Where(x => x.SubmissionId != null)
                    .Select(x => x.SubmissionId!)
                    .ToListAsync(cancellationToken);
                var existingSubmissionIds = existingIdList.ToHashSet();

                var dataRows = rows.Skip(1).ToList();
                int totalRows = dataRows.Count;
                int newCount = 0;
                int skippedCount = 0;
                int failedCount = 0;
                int totalAttachments = 0;

                var newFeedbacks = new List<UserFeedback>();

                foreach (var row in dataRows)
                {
                    // Lấy SubmissionId
                    var subId = GetColValue(row, colMap, "submissionid")?.Trim();
                    if (string.IsNullOrWhiteSpace(subId))
                    {
                        // SubmissionId trống -> Bỏ qua dòng này
                        failedCount++;
                        _logger.LogWarning("Skip row without SubmissionId: {Row}", string.Join(", ", row.Take(3)));
                        continue;
                    }

                    if (existingSubmissionIds.Contains(subId))
                    {
                        // Đã có trong SQL -> Bỏ qua
                        skippedCount++;
                        continue;
                    }

                    // Đánh dấu đã thấy để tránh trường hợp trùng ID ngay trong chính Sheet
                    existingSubmissionIds.Add(subId);

                    try
                    {
                        // Trích xuất các trường dữ liệu
                        var createdTimeStr = GetColValue(row, colMap, "createdat");
                        DateTime createdAt = DateTime.Now;
                        if (!string.IsNullOrWhiteSpace(createdTimeStr))
                        {
                            if (!DateTime.TryParse(createdTimeStr, new CultureInfo("vi-VN"), DateTimeStyles.None, out createdAt))
                            {
                                if (!DateTime.TryParse(createdTimeStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out createdAt))
                                {
                                    createdAt = DateTime.Now;
                                }
                            }
                        }

                        var feedbackType = GetColValue(row, colMap, "feedbacktype");
                        var fullName = GetColValue(row, colMap, "fullname") ?? "Ẩn danh";
                        var factory = GetColValue(row, colMap, "factory");
                        var department = GetColValue(row, colMap, "department") ?? "---";
                        var phone = GetColValue(row, colMap, "phonenumber");
                        var rawIssueGroups = GetColValue(row, colMap, "issuegroups");
                        var content = GetColValue(row, colMap, "content") ?? "";
                        var relatedParties = GetColValue(row, colMap, "relatedparties");
                        var incidentTimeLoc = GetColValue(row, colMap, "incidenttimelocation");
                        var financialLoss = GetColValue(row, colMap, "hasfinancialloss");
                        var estimatedLoss = GetColValue(row, colMap, "estimatedlossamount");
                        var hasEvidence = GetColValue(row, colMap, "hasevidence");
                        var evidenceDesc = GetColValue(row, colMap, "evidencedescription");
                        var attachmentLinks = GetColValue(row, colMap, "attachmentlink");

                        // Chuẩn hóa tiêu đề: lấy mô tả ngắn hoặc hình thức
                        var title = !string.IsNullOrWhiteSpace(content)
                            ? (content.Length > 80 ? content.Substring(0, 80) + "..." : content)
                            : (!string.IsNullOrWhiteSpace(feedbackType) ? feedbackType : "Ý kiến phản ánh");

                        var feedback = new UserFeedback
                        {
                            Id = Guid.NewGuid(),
                            SubmissionId = subId,
                            CreatedAt = createdAt,
                            FullName = fullName,
                            Factory = factory,
                            Department = department,
                            PhoneNumber = phone,
                            Title = title,
                            Content = content,
                            FeedbackType = feedbackType,
                            IssueGroups = NormalizeIssueGroups(rawIssueGroups),
                            RelatedParties = relatedParties,
                            IncidentTimeLocation = incidentTimeLoc,
                            HasFinancialLoss = financialLoss,
                            EstimatedLossAmount = estimatedLoss,
                            HasEvidence = hasEvidence,
                            EvidenceDescription = evidenceDesc,
                            Status = FeedbackStatus.New
                        };

                        // Trích xuất đính kèm (Drive files)
                        if (!string.IsNullOrWhiteSpace(attachmentLinks))
                        {
                            var fileIds = ExtractDriveFileIds(attachmentLinks);
                            foreach (var fid in fileIds)
                            {
                                var meta = await _driveService.GetFileMetadataAsync(fid);
                                var realName = meta?.Name ?? $"file_{fid.Substring(0, Math.Min(8, fid.Length))}";
                                var mimeType = meta?.MimeType ?? "application/octet-stream";
                                var size = meta?.Size ?? 0;

                                string relativeFilePath = $"https://drive.google.com/file/d/{fid}/view";

                                // Tải và lưu tệp trực tiếp vào máy chủ (uploads/feedbacks) để lưu trữ vĩnh viễn
                                try
                                {
                                    var uploadDir = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "feedbacks");
                                    if (!Directory.Exists(uploadDir))
                                    {
                                        Directory.CreateDirectory(uploadDir);
                                    }

                                    var fileBytes = await _driveService.GetFileBytesAsync(fid);
                                    if (fileBytes != null && fileBytes.Length > 0)
                                    {
                                        // Kiểm tra bảo mật toàn diện: Magic Bytes, chống giả mạo tên file, chống mã độc
                                        var (isValid, reason, detectedMime) = FileSecurityHelper.ValidateFile(fileBytes, realName);
                                        if (!isValid)
                                        {
                                            _logger.LogError("CHẶN TỆP ĐỘC HẠI: Tệp {FileId} ({FileName}) bị từ chối. Lý do: {Reason}", fid, realName, reason);
                                            feedback.Attachments.Add(new UserFeedbackAttachment
                                            {
                                                Id = Guid.NewGuid(),
                                                FeedbackId = feedback.Id,
                                                GoogleDriveFileId = fid,
                                                DriveUrl = $"https://drive.google.com/file/d/{fid}/view",
                                                FilePath = "BLOCKED_UNSAFE_FILE",
                                                FileName = realName,
                                                OriginalFileName = realName,
                                                FileType = detectedMime,
                                                FileSize = fileBytes.Length,
                                                CreatedAt = createdAt,
                                                SyncStatus = "BlockedUnsafe",
                                                ErrorMessage = reason
                                            });
                                            totalAttachments++;
                                            continue;
                                        }

                                        size = fileBytes.Length;
                                        mimeType = detectedMime;

                                        var ext = Path.GetExtension(realName);
                                        if (string.IsNullOrEmpty(ext)) ext = mimeType == "application/pdf" ? ".pdf" : ".jpg";
                                        var serverFileName = $"{fid}{ext}";
                                        var physicalPath = Path.Combine(uploadDir, serverFileName);
                                        await File.WriteAllBytesAsync(physicalPath, fileBytes, cancellationToken);
                                        relativeFilePath = $"/uploads/feedbacks/{serverFileName}";
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning(ex, "Không thể tải tệp {FileId} từ Drive về lưu trên server disk: {Message}", fid, ex.Message);
                                }

                                feedback.Attachments.Add(new UserFeedbackAttachment
                                {
                                    Id = Guid.NewGuid(),
                                    FeedbackId = feedback.Id,
                                    GoogleDriveFileId = fid,
                                    DriveUrl = $"https://drive.google.com/file/d/{fid}/view",
                                    FilePath = relativeFilePath,
                                    FileName = realName,
                                    OriginalFileName = realName,
                                    FileType = mimeType,
                                    FileSize = size,
                                    CreatedAt = createdAt,
                                    SyncStatus = relativeFilePath.StartsWith("/uploads") ? "LocalSaved" : "Synced"
                                });
                                totalAttachments++;
                            }
                        }

                        newFeedbacks.Add(feedback);
                        newCount++;
                    }
                    catch (Exception rowEx)
                    {
                        failedCount++;
                        _logger.LogError(rowEx, "Lỗi xử lý dòng có SubmissionId: {SubmissionId}", subId);
                    }
                }

                // 4. Lưu vào Database
                if (newFeedbacks.Count > 0)
                {
                    await _context.tbl_UserFeedbacks.AddRangeAsync(newFeedbacks, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                }

                // Cập nhật SyncLog
                syncLog.Status = failedCount > 0 && newCount == 0 ? "Failed" : "Success";
                syncLog.CompletedAt = DateTime.Now;
                syncLog.TotalRows = totalRows;
                syncLog.NewRows = newCount;
                syncLog.SkippedRows = skippedCount;
                syncLog.FailedRows = failedCount;
                syncLog.AttachmentCount = totalAttachments;
                await _context.SaveChangesAsync(cancellationToken);

                return new FeedbackSyncResultDto
                {
                    Success = true,
                    TotalRows = totalRows,
                    NewCount = newCount,
                    SkippedCount = skippedCount,
                    FailedCount = failedCount,
                    AttachmentCount = totalAttachments,
                    LastSyncTime = syncLog.CompletedAt,
                    Message = $"Đồng bộ hoàn tất: {newCount} mới, {skippedCount} đã tồn tại, {failedCount} bỏ qua."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong quá trình FeedbackSyncService.SyncAsync");
                syncLog.Status = "Failed";
                syncLog.CompletedAt = DateTime.Now;
                syncLog.ErrorMessage = ex.Message;
                await _context.SaveChangesAsync(cancellationToken);

                return new FeedbackSyncResultDto
                {
                    Success = false,
                    Message = $"Đồng bộ thất bại: {ex.Message}",
                    LastSyncTime = DateTime.Now
                };
            }
            finally
            {
                _syncLock.Release();
            }
        }

        public async Task<FeedbackSyncStatusDto> GetSyncStatusAsync()
        {
            var lastLog = await _context.tbl_UserFeedbackSyncLogs
                .OrderByDescending(x => x.StartedAt)
                .FirstOrDefaultAsync();

            if (lastLog == null)
            {
                return new FeedbackSyncStatusDto
                {
                    Status = "None",
                    LastSyncTime = null
                };
            }

            return new FeedbackSyncStatusDto
            {
                LastSyncTime = lastLog.CompletedAt ?? lastLog.StartedAt,
                Status = lastLog.Status,
                TotalRows = lastLog.TotalRows,
                NewRows = lastLog.NewRows,
                SkippedRows = lastLog.SkippedRows,
                FailedRows = lastLog.FailedRows,
                AttachmentCount = lastLog.AttachmentCount,
                TriggerType = lastLog.TriggerType,
                ErrorMessage = lastLog.ErrorMessage
            };
        }

        // ==================== CÁC HÀM TIỆN ÍCH XỬ LÝ NỘI DUNG ====================

        public static string NormalizeIssueGroups(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

            var items = raw.Split(new[] { ',', '\n', '\r', ';' }, StringSplitOptions.RemoveEmptyEntries)
                           .Select(s => s.Trim())
                           .Where(s => !string.IsNullOrWhiteSpace(s))
                           .Distinct();

            return string.Join("\n", items);
        }

        public static List<string> ParseIssueGroups(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return new List<string>();

            return raw.Split(new[] { '\n', '\r', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                      .Select(s => s.Trim())
                      .Where(s => !string.IsNullOrWhiteSpace(s))
                      .Distinct()
                      .ToList();
        }

        private static List<string> ExtractDriveFileIds(string text)
        {
            var result = new HashSet<string>();
            if (string.IsNullOrWhiteSpace(text)) return result.ToList();

            // Match patterns like /d/FILE_ID, open?id=FILE_ID, id=FILE_ID
            var matches = Regex.Matches(text, @"(?:id=|\/d\/|open\?id=)([a-zA-Z0-9_-]{20,})");
            foreach (Match m in matches)
            {
                if (m.Groups.Count > 1)
                {
                    result.Add(m.Groups[1].Value);
                }
            }

            // Fallback nếu chuỗi chính là FileId thuần túy
            var trimmed = text.Trim();
            if (result.Count == 0 && Regex.IsMatch(trimmed, @"^[a-zA-Z0-9_-]{25,}$"))
            {
                result.Add(trimmed);
            }

            return result.ToList();
        }

        private static Dictionary<string, int> MapHeaders(List<string> headers)
        {
            var map = new Dictionary<string, int>();

            for (int i = 0; i < headers.Count; i++)
            {
                var h = headers[i].Trim().ToLower();

                if (h.Contains("submissionid")) map["submissionid"] = i;
                else if (h.Contains("thời gian gửi") || h.Contains("timestamp")) map["createdat"] = i;
                else if (h.Contains("hình thức") || h.Contains("câu 1")) map["feedbacktype"] = i;
                else if (h.Contains("họ và tên") || h.Contains("họ tên")) map["fullname"] = i;
                else if (h.Contains("factory") || h.Contains("nhà máy")) map["factory"] = i;
                else if (h.Contains("department") || h.Contains("phòng ban") || h.Contains("bộ phận/người liên quan") == false && h.Contains("phòng")) map["department"] = i;
                else if (h.Contains("số điện thoại") || h.Contains("điện thoại") || h.Contains("sđt") || h.Contains("phone")) map["phonenumber"] = i;
                else if (h.Contains("nhóm vấn đề") || h.Contains("câu 2")) map["issuegroups"] = i;
                else if (h.Contains("mô tả sự việc") || h.Contains("câu 3") || h.Contains("nội dung")) map["content"] = i;
                else if (h.Contains("liên quan") && (h.Contains("người") || h.Contains("câu 4") || h.Contains("bộ phận"))) map["relatedparties"] = i;
                else if (h.Contains("thời gian & địa điểm") || h.Contains("câu 5") || (h.Contains("thời gian") && h.Contains("địa điểm"))) map["incidenttimelocation"] = i;
                else if (h.Contains("thiệt hại tài chính") || h.Contains("câu 6")) map["hasfinancialloss"] = i;
                else if (h.Contains("ước tính") || h.Contains("giá trị")) map["estimatedlossamount"] = i;
                else if (h.Contains("bằng chứng không") || h.Contains("câu 7")) map["hasevidence"] = i;
                else if (h.Contains("mô tả bằng chứng")) map["evidencedescription"] = i;
                else if (h.Contains("đính kèm") || h.Contains("drive") || h.Contains("link")) map["attachmentlink"] = i;
            }

            return map;
        }

        private static string? GetColValue(List<string> row, Dictionary<string, int> colMap, string key)
        {
            if (colMap.TryGetValue(key, out int idx) && idx < row.Count)
            {
                var val = row[idx]?.Trim();
                return string.IsNullOrWhiteSpace(val) ? null : val;
            }
            return null;
        }

        private static List<List<string>> ParseCsv(string csvText)
        {
            var records = new List<List<string>>();
            var currentRecord = new List<string>();
            var currentField = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < csvText.Length; i++)
            {
                char c = csvText[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < csvText.Length && csvText[i + 1] == '"')
                        {
                            currentField.Append('"');
                            i++; // Bỏ qua quote escape
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        currentField.Append(c);
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else if (c == ',')
                    {
                        currentRecord.Add(currentField.ToString());
                        currentField.Clear();
                    }
                    else if (c == '\r')
                    {
                        if (i + 1 < csvText.Length && csvText[i + 1] == '\n')
                        {
                            i++;
                        }
                        currentRecord.Add(currentField.ToString());
                        currentField.Clear();
                        records.Add(currentRecord);
                        currentRecord = new List<string>();
                    }
                    else if (c == '\n')
                    {
                        currentRecord.Add(currentField.ToString());
                        currentField.Clear();
                        records.Add(currentRecord);
                        currentRecord = new List<string>();
                    }
                    else
                    {
                        currentField.Append(c);
                    }
                }
            }

            if (currentField.Length > 0 || currentRecord.Count > 0)
            {
                currentRecord.Add(currentField.ToString());
                records.Add(currentRecord);
            }

            return records;
        }
    }

    internal static class SyncLogExtensions
    {
        public static void MessageUpdate(this UserFeedbackSyncLog log, string msg)
        {
            log.ErrorMessage = msg;
        }
    }
}
