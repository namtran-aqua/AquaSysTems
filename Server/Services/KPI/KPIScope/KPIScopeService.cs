using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AquaSolution.Data.Connection;
using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Data.Entities.Admin;
using AquaSolution.Data.Data.Entities.KPI;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.KPI.Scope;

namespace AquaSolution.Server.Services.KPI.KPIScope
{
    public class KPIScopeService : IKPIScopeService
    {
        private readonly AquaDbContext _context;

        public KPIScopeService(AquaDbContext context)
        {
            _context = context;
        }

        public async Task<List<KPIScopeDto>> GetScopesAsync(int? year = null, Guid? departmentId = null, Guid? userId = null, bool? isActive = null)
        {
            var query = _context.tbl_KPIEmployeeScopes
                .AsNoTracking()
                .Include(s => s.ExemptMonths)
                .AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(s => s.UserId == userId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(s => s.IsActive == isActive.Value);
            }

            var scopes = await query.OrderByDescending(s => s.CreatedDate).ToListAsync();

            if (year.HasValue)
            {
                scopes = scopes.Where(s => s.ExemptMonths.Any(m => m.Year == year.Value)
                                          || (s.EffectiveDate.HasValue && s.EffectiveDate.Value.Year == year.Value)
                                          || (s.StartDate.HasValue && s.StartDate.Value.Year == year.Value)).ToList();
            }

            // Get user information & departments
            var userIds = scopes.Select(s => s.UserId).Distinct().ToList();
            var users = await _context.tbl_Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            var deptIds = users.Values.Where(u => u.DepartmentId.HasValue).Select(u => u.DepartmentId.Value).Distinct().ToList();
            var depts = await _context.tbl_Departments
                .AsNoTracking()
                .Where(d => deptIds.Contains(d.Id))
                .ToDictionaryAsync(d => d.Id, d => d.Name);

            if (departmentId.HasValue)
            {
                scopes = scopes.Where(s => users.TryGetValue(s.UserId, out var u) && u.DepartmentId == departmentId.Value).ToList();
            }

            var result = scopes.Select(s =>
            {
                users.TryGetValue(s.UserId, out var user);
                string deptName = (user != null && user.DepartmentId.HasValue && depts.TryGetValue(user.DepartmentId.Value, out var dn)) ? dn : string.Empty;

                return new KPIScopeDto
                {
                    Id = s.Id,
                    UserId = s.UserId,
                    UserName = user?.WorkDayId ?? string.Empty,
                    UserFullName = user?.FullName ?? string.Empty,
                    DepartmentName = deptName,
                    ScopeType = s.ScopeType,
                    ScopeTypeName = GetScopeTypeName(s.ScopeType),
                    EffectiveDate = s.EffectiveDate,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    Reason = s.Reason,
                    IsActive = s.IsActive,
                    CreatedBy = s.CreatedBy,
                    CreatedDate = s.CreatedDate,
                    UpdatedBy = s.UpdatedBy,
                    UpdatedDate = s.UpdatedDate,
                    ExemptMonths = s.ExemptMonths.Select(m => new KPIExemptMonthDto
                    {
                        Id = m.Id,
                        ScopeId = m.ScopeId,
                        UserId = m.UserId,
                        Year = m.Year,
                        Month = m.Month,
                        Note = m.Note,
                        CreatedDate = m.CreatedDate
                    }).OrderBy(m => m.Year).ThenBy(m => m.Month).ToList()
                };
            }).ToList();

            return result;
        }

        public async Task<KPIScopeDto> GetScopeByIdAsync(Guid id)
        {
            var s = await _context.tbl_KPIEmployeeScopes
                .AsNoTracking()
                .Include(x => x.ExemptMonths)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (s == null) return null;

            var user = await _context.tbl_Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == s.UserId);
            string deptName = string.Empty;
            if (user?.DepartmentId != null)
            {
                var dept = await _context.tbl_Departments.AsNoTracking().FirstOrDefaultAsync(d => d.Id == user.DepartmentId.Value);
                deptName = dept?.Name ?? string.Empty;
            }

            return new KPIScopeDto
            {
                Id = s.Id,
                UserId = s.UserId,
                UserName = user?.WorkDayId ?? string.Empty,
                UserFullName = user?.FullName ?? string.Empty,
                DepartmentName = deptName,
                ScopeType = s.ScopeType,
                ScopeTypeName = GetScopeTypeName(s.ScopeType),
                EffectiveDate = s.EffectiveDate,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                Reason = s.Reason,
                IsActive = s.IsActive,
                CreatedBy = s.CreatedBy,
                CreatedDate = s.CreatedDate,
                UpdatedBy = s.UpdatedBy,
                UpdatedDate = s.UpdatedDate,
                ExemptMonths = s.ExemptMonths.Select(m => new KPIExemptMonthDto
                {
                    Id = m.Id,
                    ScopeId = m.ScopeId,
                    UserId = m.UserId,
                    Year = m.Year,
                    Month = m.Month,
                    Note = m.Note,
                    CreatedDate = m.CreatedDate
                }).OrderBy(m => m.Year).ThenBy(m => m.Month).ToList()
            };
        }

        public async Task<KPIScopeDto> CreateScopeAsync(KPIScopeCreateOrUpdateDto dto, Guid currentUserId)
        {
            using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                var scope = new KPIEmployeeScope
                {
                    Id = Guid.NewGuid(),
                    UserId = dto.UserId,
                    ScopeType = dto.ScopeType,
                    EffectiveDate = dto.EffectiveDate,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    Reason = dto.Reason,
                    IsActive = dto.IsActive,
                    CreatedBy = currentUserId,
                    CreatedDate = DateTime.Now
                };

                await _context.tbl_KPIEmployeeScopes.AddAsync(scope);

                if (dto.IsActive && dto.ExemptMonths != null && dto.ExemptMonths.Count > 0)
                {
                    foreach (var m in dto.ExemptMonths)
                    {
                        // Remove any existing exempt month for this user/year/month to avoid unique constraint collision
                        var existing = await _context.tbl_KPIEmployeeExemptMonths
                            .FirstOrDefaultAsync(x => x.UserId == dto.UserId && x.Year == m.Year && x.Month == m.Month);
                        if (existing != null)
                        {
                            _context.tbl_KPIEmployeeExemptMonths.Remove(existing);
                        }

                        var exempt = new KPIEmployeeExemptMonth
                        {
                            Id = Guid.NewGuid(),
                            ScopeId = scope.Id,
                            UserId = dto.UserId,
                            Year = m.Year,
                            Month = m.Month,
                            Note = m.Note,
                            CreatedDate = DateTime.Now
                        };
                        await _context.tbl_KPIEmployeeExemptMonths.AddAsync(exempt);
                    }
                }

                await _context.SaveChangesAsync();
                await trans.CommitAsync();

                return await GetScopeByIdAsync(scope.Id);
            }
            catch
            {
                await trans.RollbackAsync();
                throw;
            }
        }

        public async Task<KPIScopeDto> UpdateScopeAsync(KPIScopeCreateOrUpdateDto dto, Guid currentUserId)
        {
            if (!dto.Id.HasValue) throw new ArgumentException("Id is required for update.");

            using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                var scope = await _context.tbl_KPIEmployeeScopes
                    .Include(s => s.ExemptMonths)
                    .FirstOrDefaultAsync(s => s.Id == dto.Id.Value);

                if (scope == null) throw new InvalidOperationException("Hồ sơ không tồn tại.");

                scope.UserId = dto.UserId;
                scope.ScopeType = dto.ScopeType;
                scope.EffectiveDate = dto.EffectiveDate;
                scope.StartDate = dto.StartDate;
                scope.EndDate = dto.EndDate;
                scope.Reason = dto.Reason;
                scope.IsActive = dto.IsActive;
                scope.UpdatedBy = currentUserId;
                scope.UpdatedDate = DateTime.Now;

                // Remove existing exempt months for this scope
                _context.tbl_KPIEmployeeExemptMonths.RemoveRange(scope.ExemptMonths);

                // Re-add exempt months if scope is active
                if (dto.IsActive && dto.ExemptMonths != null && dto.ExemptMonths.Count > 0)
                {
                    foreach (var m in dto.ExemptMonths)
                    {
                        // Remove conflicting record if any
                        var conflict = await _context.tbl_KPIEmployeeExemptMonths
                            .FirstOrDefaultAsync(x => x.UserId == dto.UserId && x.Year == m.Year && x.Month == m.Month && x.ScopeId != scope.Id);
                        if (conflict != null)
                        {
                            _context.tbl_KPIEmployeeExemptMonths.Remove(conflict);
                        }

                        var exempt = new KPIEmployeeExemptMonth
                        {
                            Id = Guid.NewGuid(),
                            ScopeId = scope.Id,
                            UserId = dto.UserId,
                            Year = m.Year,
                            Month = m.Month,
                            Note = m.Note,
                            CreatedDate = DateTime.Now
                        };
                        await _context.tbl_KPIEmployeeExemptMonths.AddAsync(exempt);
                    }
                }

                await _context.SaveChangesAsync();
                await trans.CommitAsync();

                return await GetScopeByIdAsync(scope.Id);
            }
            catch
            {
                await trans.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeactivateScopeAsync(Guid scopeId, Guid currentUserId)
        {
            using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                var scope = await _context.tbl_KPIEmployeeScopes
                    .Include(s => s.ExemptMonths)
                    .FirstOrDefaultAsync(s => s.Id == scopeId);

                if (scope == null) return false;

                scope.IsActive = false;
                scope.UpdatedBy = currentUserId;
                scope.UpdatedDate = DateTime.Now;

                // Remove exempt months so they no longer apply
                if (scope.ExemptMonths != null && scope.ExemptMonths.Count > 0)
                {
                    _context.tbl_KPIEmployeeExemptMonths.RemoveRange(scope.ExemptMonths);
                }

                await _context.SaveChangesAsync();
                await trans.CommitAsync();
                return true;
            }
            catch
            {
                await trans.RollbackAsync();
                throw;
            }
        }

        public List<ExemptMonthItem> SuggestExemptMonths(KPIScopeType scopeType, DateTime? effectiveDate, DateTime? startDate, DateTime? endDate)
        {
            var result = new List<ExemptMonthItem>();

            switch (scopeType)
            {
                case KPIScopeType.NewEmployee:
                    if (effectiveDate.HasValue)
                    {
                        int year = effectiveDate.Value.Year;
                        int hireMonth = effectiveDate.Value.Month;
                        // Tháng vào làm (dù đầu hay giữa tháng) được miễn KPI
                        // Các tháng từ tháng 1 đến hết tháng vào làm đều miễn
                        for (int m = 1; m <= hireMonth; m++)
                        {
                            result.Add(new ExemptMonthItem
                            {
                                Year = year,
                                Month = m,
                                Note = $"Nhân viên mới (vào làm {effectiveDate.Value:dd/MM/yyyy})"
                            });
                        }
                    }
                    break;

                case KPIScopeType.MaternityLeave:
                case KPIScopeType.LongTermLeave:
                case KPIScopeType.Other:
                    if (startDate.HasValue && endDate.HasValue && startDate.Value <= endDate.Value)
                    {
                        var cur = new DateTime(startDate.Value.Year, startDate.Value.Month, 1);
                        var end = new DateTime(endDate.Value.Year, endDate.Value.Month, 1);
                        string label = scopeType == KPIScopeType.MaternityLeave ? "Nghỉ thai sản"
                            : (scopeType == KPIScopeType.LongTermLeave ? "Nghỉ dài hạn" : "Miễn đặc biệt");

                        while (cur <= end)
                        {
                            result.Add(new ExemptMonthItem
                            {
                                Year = cur.Year,
                                Month = cur.Month,
                                Note = $"{label} ({startDate.Value:dd/MM/yyyy} - {endDate.Value:dd/MM/yyyy})"
                            });
                            cur = cur.AddMonths(1);
                        }
                    }
                    break;
            }

            return result;
        }

        public async Task<List<KPIComplianceMatrixDto>> GetMonthlyComplianceMatrixAsync(int year, Guid? departmentId)
        {
            // 1. Get active users that have a position (only employees with positions participate in KPI)
            var userQuery = _context.tbl_Users
                .AsNoTracking()
                .Where(u => !u.IsDeleted && u.IsActive && u.PositionId.HasValue && u.PositionId.Value != Guid.Empty);

            if (departmentId.HasValue)
            {
                userQuery = userQuery.Where(u => u.DepartmentId == departmentId.Value);
            }

            var users = await userQuery.OrderBy(u => u.FullName).ToListAsync();
            var userIds = users.Select(u => u.Id).ToList();

            // 2. Get department and position names
            var deptIds = users.Where(u => u.DepartmentId.HasValue).Select(u => u.DepartmentId.Value).Distinct().ToList();
            var departments = await _context.tbl_Departments
                .AsNoTracking()
                .Where(d => deptIds.Contains(d.Id))
                .ToDictionaryAsync(d => d.Id, d => d.Name);

            var posIds = users.Where(u => u.PositionId.HasValue).Select(u => u.PositionId.Value).Distinct().ToList();
            var positions = await _context.tbl_Positions
                .AsNoTracking()
                .Where(p => posIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Name);

            // 3. Get all exempt months for these users in this year
            var exemptMonths = await _context.tbl_KPIEmployeeExemptMonths
                .AsNoTracking()
                .Where(m => m.Year == year && userIds.Contains(m.UserId))
                .ToListAsync();
            var exemptMap = exemptMonths
                .GroupBy(m => m.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(x => x.Month)
                          .ToDictionary(
                              mg => mg.Key,
                              mg => mg.OrderByDescending(x => x.CreatedDate).First().Note
                          )
                );

            // 4. Get monthly scores for these users in this year
            var scores = await _context.tbl_KPITotalScores
                .AsNoTracking()
                .Where(s => s.Year == year && s.Month.HasValue && s.Month.Value >= 1 && s.Month.Value <= 12
                            && userIds.Contains(s.CreatedBy) && s.kPITotalScoreType == KPITotalScoreType.Staff)
                .ToListAsync();

            var scoreMap = scores
                .GroupBy(s => s.CreatedBy)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(x => x.Month.Value)
                          .ToDictionary(
                              mg => mg.Key,
                              mg => mg.OrderByDescending(x => x.Status == StatusKPIRequestType.Approved)
                                      .ThenByDescending(x => x.CreatedDate)
                                      .First()
                          )
                );

            // Current date to evaluate past vs future missing months
            var now = DateTime.Now;

            // 5. Build compliance matrix
            var matrix = new List<KPIComplianceMatrixDto>();

            foreach (var user in users)
            {
                string deptName = (user.DepartmentId.HasValue && departments.TryGetValue(user.DepartmentId.Value, out var dn)) ? dn : string.Empty;
                string posName = (user.PositionId.HasValue && positions.TryGetValue(user.PositionId.Value, out var pn)) ? pn : string.Empty;

                var row = new KPIComplianceMatrixDto
                {
                    UserId = user.Id,
                    UserName = user.WorkDayId ?? string.Empty,
                    UserFullName = user.FullName ?? string.Empty,
                    DepartmentId = user.DepartmentId,
                    DepartmentName = deptName,
                    PositionId = user.PositionId,
                    PositionName = posName,
                    Year = year
                };

                exemptMap.TryGetValue(user.Id, out var userExempts);
                scoreMap.TryGetValue(user.Id, out var userScores);

                for (int m = 1; m <= 12; m++)
                {
                    var monthDto = new KPIComplianceMonthDto { Month = m };

                    if (userExempts != null && userExempts.TryGetValue(m, out var note))
                    {
                        monthDto.Status = MonthKPIStatus.NotApplicable;
                        monthDto.StatusName = "Miễn KPI";
                        monthDto.Note = note;
                        row.TotalExemptMonths++;
                    }
                    else if (userScores != null && userScores.TryGetValue(m, out var score))
                    {
                        monthDto.TaskSubmitedId = score.SubmitId;
                        monthDto.TotalScore = (double?)score.TotaleScore;

                        switch (score.Status)
                        {
                            case StatusKPIRequestType.Approved:
                                monthDto.Status = MonthKPIStatus.Approved;
                                monthDto.StatusName = "Đã duyệt";
                                row.TotalApprovedMonths++;
                                row.TotalSubmittedMonths++;
                                break;
                            case StatusKPIRequestType.WaitingForApproval:
                                monthDto.Status = MonthKPIStatus.Pending;
                                monthDto.StatusName = "Chờ duyệt";
                                row.TotalPendingMonths++;
                                row.TotalSubmittedMonths++;
                                break;
                            case StatusKPIRequestType.Rejected:
                                monthDto.Status = MonthKPIStatus.Rejected;
                                monthDto.StatusName = "Từ chối";
                                row.TotalSubmittedMonths++;
                                break;
                            default:
                                monthDto.Status = MonthKPIStatus.Pending;
                                monthDto.StatusName = "Đang soạn";
                                row.TotalPendingMonths++;
                                row.TotalSubmittedMonths++;
                                break;
                        }
                    }
                    else
                    {
                        // Kiểm tra kỳ đánh giá đã đến chưa:
                        // Đánh giá KPI tháng M diễn ra vào đầu tháng M+1.
                        // Ví dụ tháng hiện tại là 10:
                        // - Tháng 1..9 đã qua hạn nộp -> nếu chưa có điểm thì tính là "Chưa nộp" (Missing)
                        // - Tháng 10, 11, 12 chưa hết kỳ/chưa đến hạn nộp -> hiển thị "Chưa đến kỳ" (Open)
                        bool isPastEvaluation = year < now.Year || (year == now.Year && m < now.Month);
                        if (isPastEvaluation)
                        {
                            monthDto.Status = MonthKPIStatus.Missing;
                            monthDto.StatusName = "Chưa nộp";
                            row.TotalMissingMonths++;
                        }
                        else
                        {
                            monthDto.Status = MonthKPIStatus.Open;
                            monthDto.StatusName = "Chưa đến kỳ";
                            row.TotalOpenMonths++;
                        }
                    }

                    row.Months.Add(monthDto);
                }

                matrix.Add(row);
            }

            return matrix;
        }

        public async Task<List<KPIExemptMonthDto>> GetUserExemptMonthsAsync(Guid userId, int year)
        {
            var months = await _context.tbl_KPIEmployeeExemptMonths
                .AsNoTracking()
                .Where(m => m.UserId == userId && m.Year == year)
                .OrderBy(m => m.Month)
                .Select(m => new KPIExemptMonthDto
                {
                    Id = m.Id,
                    ScopeId = m.ScopeId,
                    UserId = m.UserId,
                    Year = m.Year,
                    Month = m.Month,
                    Note = m.Note,
                    CreatedDate = m.CreatedDate
                })
                .ToListAsync();

            return months;
        }

        private static string GetScopeTypeName(KPIScopeType type)
        {
            return type switch
            {
                KPIScopeType.NewEmployee => "Nhân viên mới",
                KPIScopeType.MaternityLeave => "Nghỉ thai sản",
                KPIScopeType.LongTermLeave => "Nghỉ dài hạn",
                KPIScopeType.Other => "Khác",
                _ => type.ToString()
            };
        }
    }
}
