using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AquaSolution.Data.Connection;
using AquaSolution.Data.Data.Entities.Feedbacks;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Feedbacks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AquaSolution.Server.Services.Feedbacks
{
    public class UserFeedbackService : IUserFeedbackService
    {
        private readonly AquaDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<UserFeedbackService> _logger;
        private static bool _tablesEnsured = false;
        private static readonly object _lock = new();

        public UserFeedbackService(
            AquaDbContext context,
            IWebHostEnvironment env,
            ILogger<UserFeedbackService> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
        }

        public async Task EnsureDatabaseTablesAsync()
        {
            if (_tablesEnsured) return;

            try
            {
                var sql = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tbl_UserFeedbacks')
BEGIN
    CREATE TABLE tbl_UserFeedbacks (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        FullName NVARCHAR(250) NOT NULL,
        Age INT NULL,
        Factory NVARCHAR(250) NULL,
        Department NVARCHAR(250) NOT NULL,
        PhoneNumber NVARCHAR(50) NULL,
        Title NVARCHAR(500) NOT NULL,
        Content NVARCHAR(MAX) NOT NULL,
        Status INT NOT NULL DEFAULT 1,
        AdminNote NVARCHAR(2000) NULL,
        ProcessedBy NVARCHAR(250) NULL,
        ProcessedAt DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
    );
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbl_UserFeedbacks') AND name = 'Factory')
BEGIN
    ALTER TABLE tbl_UserFeedbacks ADD Factory NVARCHAR(250) NULL;
END

UPDATE tbl_UserFeedbacks SET Factory = '' WHERE Factory IS NULL;

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tbl_UserFeedbackAttachments')
BEGIN
    CREATE TABLE tbl_UserFeedbackAttachments (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        FeedbackId UNIQUEIDENTIFIER NOT NULL,
        FileName NVARCHAR(500) NOT NULL,
        OriginalFileName NVARCHAR(500) NOT NULL,
        FilePath NVARCHAR(1000) NOT NULL,
        FileType NVARCHAR(100) NULL,
        FileSize BIGINT NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_tbl_UserFeedbackAttachments_FeedbackId FOREIGN KEY (FeedbackId) 
            REFERENCES tbl_UserFeedbacks(Id) ON DELETE CASCADE
    );
END";
                await _context.Database.ExecuteSqlRawAsync(sql);
                _tablesEnsured = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking/creating tbl_UserFeedbacks tables");
            }
        }

        public async Task<UserFeedbackDto> SubmitFeedbackAsync(UserFeedbackCreateDto dto)
        {
            await EnsureDatabaseTablesAsync();

            var feedback = new UserFeedback
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName.Trim(),
                Age = dto.Age,
                Factory = (dto.Factory ?? "").Trim(),
                Department = dto.Department.Trim(),
                PhoneNumber = dto.PhoneNumber?.Trim(),
                Title = dto.Title.Trim(),
                Content = dto.Content.Trim(),
                Status = FeedbackStatus.New,
                CreatedAt = DateTime.Now
            };

            if (dto.Attachments != null && dto.Attachments.Count > 0)
            {
                foreach (var att in dto.Attachments)
                {
                    feedback.Attachments.Add(new UserFeedbackAttachment
                    {
                        Id = Guid.NewGuid(),
                        FeedbackId = feedback.Id,
                        FileName = att.FileName,
                        OriginalFileName = att.OriginalFileName,
                        FilePath = att.FilePath,
                        FileType = att.FileType,
                        FileSize = att.FileSize,
                        CreatedAt = DateTime.Now
                    });
                }
            }

            await _context.tbl_UserFeedbacks.AddAsync(feedback);
            await _context.SaveChangesAsync();

            return MapToDto(feedback);
        }

        public async Task<FeedbackPagedResult<UserFeedbackDto>> GetPagedAsync(UserFeedbackFilterDto filter)
        {
            await EnsureDatabaseTablesAsync();

            var query = _context.tbl_UserFeedbacks
                                .Include(x => x.Attachments)
                                .AsNoTracking()
                                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();
                query = query.Where(x => x.FullName.ToLower().Contains(term) ||
                                         x.Title.ToLower().Contains(term) ||
                                         x.Content.ToLower().Contains(term) ||
                                         (x.PhoneNumber != null && x.PhoneNumber.Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(filter.Factory))
            {
                var fac = filter.Factory.Trim().ToLower();
                query = query.Where(x => x.Factory != null && x.Factory.ToLower() == fac);
            }

            if (!string.IsNullOrWhiteSpace(filter.Department))
            {
                var dept = filter.Department.Trim().ToLower();
                query = query.Where(x => x.Department.ToLower() == dept);
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(x => x.Status == filter.Status.Value);
            }

            if (filter.FromDate.HasValue)
            {
                var from = filter.FromDate.Value.Date;
                query = query.Where(x => x.CreatedAt >= from);
            }

            if (filter.ToDate.HasValue)
            {
                var to = filter.ToDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.CreatedAt <= to);
            }

            var totalCount = await query.CountAsync();

            var pageIndex = filter.PageIndex < 1 ? 1 : filter.PageIndex;
            var pageSize = filter.PageSize < 1 ? 10 : filter.PageSize;

            var items = await query.OrderByDescending(x => x.CreatedAt)
                                   .Skip((pageIndex - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();

            return new FeedbackPagedResult<UserFeedbackDto>
            {
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize,
                Items = items.Select(MapToDto).ToList()
            };
        }

        public async Task<UserFeedbackDto?> GetByIdAsync(Guid id)
        {
            await EnsureDatabaseTablesAsync();

            var feedback = await _context.tbl_UserFeedbacks
                                         .Include(x => x.Attachments)
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(x => x.Id == id);

            return feedback != null ? MapToDto(feedback) : null;
        }

        public async Task<bool> UpdateStatusAsync(UserFeedbackStatusUpdateDto dto, string currentUserName)
        {
            await EnsureDatabaseTablesAsync();

            var feedback = await _context.tbl_UserFeedbacks.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (feedback == null) return false;

            feedback.Status = dto.Status;
            feedback.AdminNote = dto.AdminNote?.Trim();
            feedback.ProcessedBy = currentUserName;
            feedback.ProcessedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            await EnsureDatabaseTablesAsync();

            var feedback = await _context.tbl_UserFeedbacks
                                         .Include(x => x.Attachments)
                                         .FirstOrDefaultAsync(x => x.Id == id);
            if (feedback == null) return false;

            // Delete physical attachment files
            foreach (var att in feedback.Attachments)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(att.FilePath))
                    {
                        var relative = att.FilePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());
                        var physical = Path.Combine(_env.WebRootPath, relative);
                        if (File.Exists(physical))
                        {
                            File.Delete(physical);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not delete physical attachment file: {Path}", att.FilePath);
                }
            }

            _context.tbl_UserFeedbacks.Remove(feedback);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<FeedbackDashboardStatsDto> GetDashboardStatsAsync()
        {
            await EnsureDatabaseTablesAsync();

            var total = await _context.tbl_UserFeedbacks.CountAsync();
            var newCount = await _context.tbl_UserFeedbacks.CountAsync(x => x.Status == FeedbackStatus.New);
            var processing = await _context.tbl_UserFeedbacks.CountAsync(x => x.Status == FeedbackStatus.Processing);
            var resolved = await _context.tbl_UserFeedbacks.CountAsync(x => x.Status == FeedbackStatus.Resolved);
            var closed = await _context.tbl_UserFeedbacks.CountAsync(x => x.Status == FeedbackStatus.Closed);

            return new FeedbackDashboardStatsDto
            {
                TotalCount = total,
                NewCount = newCount,
                ProcessingCount = processing,
                ResolvedCount = resolved,
                ClosedCount = closed
            };
        }

        public async Task<List<string>> GetDepartmentListAsync()
        {
            try
            {
                var depts = await _context.tbl_Departments
                                          .Where(d => !string.IsNullOrEmpty(d.Name))
                                          .OrderBy(d => d.Name)
                                          .Select(d => d.Name)
                                          .Distinct()
                                          .ToListAsync();

                if (depts.Count > 0) return depts;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load departments from tbl_Departments");
            }

            // Fallback default list
            return new List<string>
            {
                "Ban Giám Đốc",
                "Phòng Nhân Sự (HR)",
                "Phòng Kế Toán (Finance)",
                "Phòng IT - Công Nghệ Thông Tin",
                "Phòng Kế Hoạch (Planning)",
                "Phòng Mua Hàng (Purchasing)",
                "Bộ Phận Sản Xuất (Production)",
                "Bộ Phận Quản Lý Chất Lượng (QA/QC)",
                "Bộ Phận Bảo Trì (Maintenance)",
                "Bộ Phận Kho Vận (Warehouse/Logistics)"
            };
        }

        public async Task<List<string>> GetFactoryListAsync()
        {
            try
            {
                var factories = await _context.tbl_Factorys
                                              .Where(f => !string.IsNullOrEmpty(f.Name))
                                              .OrderBy(f => f.Name)
                                              .Select(f => f.Name)
                                              .Distinct()
                                              .ToListAsync();

                if (factories.Count > 0) return factories;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load factories from tbl_Factories");
            }

            // Fallback default list
            return new List<string>
            {
                "Nhà máy 1",
                "Nhà máy 2",
                "Nhà máy 3",
                "Văn phòng chính (Head Office)"
            };
        }

        private static UserFeedbackDto MapToDto(UserFeedback entity)
        {
            return new UserFeedbackDto
            {
                Id = entity.Id,
                FullName = entity.FullName,
                Age = entity.Age,
                Factory = entity.Factory ?? string.Empty,
                Department = entity.Department,
                PhoneNumber = entity.PhoneNumber,
                Title = entity.Title,
                Content = entity.Content,
                Status = entity.Status,
                AdminNote = entity.AdminNote,
                ProcessedBy = entity.ProcessedBy,
                ProcessedAt = entity.ProcessedAt,
                CreatedAt = entity.CreatedAt,
                Attachments = entity.Attachments?.Select(a => new UserFeedbackAttachmentDto
                {
                    Id = a.Id,
                    FeedbackId = a.FeedbackId,
                    FileName = a.FileName,
                    OriginalFileName = a.OriginalFileName,
                    FilePath = a.FilePath,
                    FileType = a.FileType,
                    FileSize = a.FileSize,
                    CreatedAt = a.CreatedAt
                }).ToList() ?? new List<UserFeedbackAttachmentDto>()
            };
        }
    }
}
