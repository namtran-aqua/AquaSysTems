using AquaSolution.Data.Data.Entities.Admin;
using AquaSolution.Data.Data.Entities.Scraps;
using AquaSolution.Data.Entities.Scraps;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.Enum.Scrap;
using AquaSolution.Shared.ReportDto;
using AquaSolution.Shared.ScrapManagement.Materials;
using AquaSolution.Shared.ScrapManagement.Scrap;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace AquaSolution.Server.Services.ScrapManagetment.ReportServices
{
    public class ReportService : IReportService
    {
        private readonly IRepository<HistoryScrap> _scrapRepo;
        private readonly IRepository<HistoryScrapDetail> _detailRepo;
        private readonly IRepository<RequestApproval> _approvalRepo;
        private readonly IRepository<FlowApprovalScrap> _flowRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Department> _departmentRepo;

        public ReportService(
            IRepository<HistoryScrap> scrapRepo,
            IRepository<HistoryScrapDetail> detailRepo,
            IRepository<RequestApproval> approvalRepo,
            IRepository<FlowApprovalScrap> flowRepo,
            IRepository<User> userRepo,
            IRepository<Department> departmentRepo)
        {
            _scrapRepo = scrapRepo;
            _detailRepo = detailRepo;
            _approvalRepo = approvalRepo;
            _flowRepo = flowRepo;
            _userRepo = userRepo;
            _departmentRepo = departmentRepo;
        }

        // ─── ApplyFilter — tất cả lọc theo HistoryScrap.CreatedDate ─────────
        private IQueryable<HistoryScrap> ApplyFilter(IQueryable<HistoryScrap> query, ReportFilterDto filter)
        {
            if (filter.FactoryId.HasValue)
                query = query.Where(x => x.FactoryId == filter.FactoryId.Value);

            switch (filter.Period)
            {
                case FilterPeriod.Week:
                    var (start, end) = GetIsoWeekRange(filter.Year, filter.Week ?? 1);
                    query = query.Where(x => x.CreatedDate >= start && x.CreatedDate < end);
                    break;

                case FilterPeriod.Month:
                    query = query.Where(x =>
                        x.CreatedDate.Year == filter.Year &&
                        x.CreatedDate.Month == filter.Month);
                    break;

                case FilterPeriod.Year:
                    query = query.Where(x => x.CreatedDate.Year == filter.Year);
                    break;
            }

            return query;
        }

        // ─── Kỳ trước ────────────────────────────────────────────────────────
        private static ReportFilterDto PreviousPeriod(ReportFilterDto filter)
        {
            var prev = new ReportFilterDto
            {
                FactoryId = filter.FactoryId,
                Period = filter.Period,
                Year = filter.Year,
                Month = filter.Month,
                Week = filter.Week
            };

            switch (filter.Period)
            {
                case FilterPeriod.Week:
                    if (filter.Week <= 1) { prev.Week = 52; prev.Year--; }
                    else prev.Week = filter.Week - 1;
                    break;
                case FilterPeriod.Month:
                    if (filter.Month <= 1) { prev.Month = 12; prev.Year--; }
                    else prev.Month = filter.Month - 1;
                    break;
                case FilterPeriod.Year:
                    prev.Year--;
                    break;
            }

            return prev;
        }

        // ─── GetIsoWeekRange — dùng ISOWeek built-in, KHÔNG tính offset thủ công ──
        private static (DateTime Start, DateTime End) GetIsoWeekRange(int year, int week)
        {
            // ISOWeek.ToDateTime xử lý đúng mọi năm kể cả năm 53 tuần (2026, 2032...)
            DateTime start = ISOWeek.ToDateTime(year, week, DayOfWeek.Monday);
            DateTime end = start.AddDays(7); // exclusive
            return (start, end);
        }

        // ─── GetReportPageAsync ───────────────────────────────────────────────
        public async Task<ReportPageDto> GetReportPageAsync(ReportFilterDto filter)
        {
            var summary = await GetSummaryAsync(filter);
            var dept = await GetDepartmentReportAsync(filter);
            var material = await GetMaterialReportAsync(filter);
            var trend = await GetTrendAsync(filter);
            var approval = await GetApprovalStatusAsync(filter);
            var pipeline = await GetPipelineAsync(filter);

            return new ReportPageDto
            {
                Summary = summary,
                DepartmentReport = dept,
                MaterialReport = material,
                Trend = trend,
                ApprovalStatus = approval,
                Pipeline = pipeline
            };
        }

        // ─── Summary ─────────────────────────────────────────────────────────
        public async Task<ReportSummaryDto> GetSummaryAsync(ReportFilterDto filter)
        {
            try
            {
                var query = ApplyFilter(_scrapRepo.Query(), filter);
                var prevQuery = ApplyFilter(_scrapRepo.Query(), PreviousPeriod(filter));

                var scraps = await query.ToListAsync();
                var prevScraps = await prevQuery.ToListAsync();

                var totalOrders = scraps.Count;
                var totalWeight = scraps.Sum(x => x.TotalAmount ?? 0);
                var confirmedWeight = scraps.Sum(x => x.ConfirmAmount ?? 0);
                var nowPending = scraps.Count(x => x.Status == StatusScrap.Pending);
                var overdue = scraps.Count(x =>
                    x.Status == StatusScrap.Pending &&
                    (DateTime.Now - x.CreatedDate).TotalDays > 3);

                var prevOrders = prevScraps.Count;
                var prevWeight = prevScraps.Sum(x => x.TotalAmount ?? 0);

                return new ReportSummaryDto
                {
                    TotalOrders = totalOrders,
                    TotalWeight = totalWeight,
                    ConfirmedWeight = confirmedWeight,
                    PendingOrders = nowPending,
                    OverduePendingOrders = overdue,
                    TotalOrdersChange = prevOrders == 0 ? 0 : Math.Round((totalOrders - prevOrders) / (double)prevOrders * 100, 1),
                    TotalWeightChange = prevWeight == 0 ? 0 : Math.Round((double)((totalWeight - prevWeight) / prevWeight) * 100, 1)
                };
            }
            catch (Exception ex) { throw ex; }
        }

        // ─── Department Report ────────────────────────────────────────────────
        public async Task<List<DepartmentReportDto>> GetDepartmentReportAsync(ReportFilterDto filter)
        {
            try
            {
                var query = ApplyFilter(_scrapRepo.Query(), filter);

                // ── B1: Group HistoryScrap theo phòng ban ─────────────────────────────
                var grouped = await query
                    .GroupBy(x => x.DepartmentId)
                    .Select(g => new
                    {
                        DepartmentId = g.Key,
                        TotalOrders = g.Count(),
                        TotalWeight = g.Sum(x => x.TotalAmount ?? 0),
                        ConfirmedWeight = g.Sum(x => x.ConfirmAmount ?? 0),

                        TotalOrderApproval = g.Count(x => x.Status == StatusScrap.Approved),
                        TotalOrderReject = g.Count(x => x.Status == StatusScrap.Rejected),
                        TotalOrderPending = g.Count(x => x.Status == StatusScrap.Pending),
                        TotalOrderDone = g.Count(x => x.Status == StatusScrap.Done),

                        // Lấy ScrapIds theo từng nhóm status để join Detail sau
                        AllScrapIds = g.Select(x => x.Id).ToList(),
                        ApprovedScrapIds = g.Where(x => x.Status == StatusScrap.Approved).Select(x => x.Id).ToList(),
                        RejectedScrapIds = g.Where(x => x.Status == StatusScrap.Rejected).Select(x => x.Id).ToList(),
                        DoneScrapIds = g.Where(x => x.Status == StatusScrap.Done).Select(x => x.Id).ToList(),
                        PendingScrapIds = g.Where(x => x.Status == StatusScrap.Pending).Select(x => x.Id).ToList(),
                    })
                    .ToListAsync();

                // ── B2: Load tất cả Detail 1 lần, group theo ScrapHistoryId ──────────
                var allScrapIds = grouped.SelectMany(g => g.AllScrapIds).Distinct().ToList();

                var detailQtyMap = await _detailRepo.Query()
                    .Where(d => allScrapIds.Contains(d.ScrapHistoryId))
                    .GroupBy(d => d.ScrapHistoryId)
                    .Select(g => new
                    {
                        ScrapId = g.Key,
                        TotalQty = g.Sum(x => x.Quantity)
                    })
                    .ToListAsync();

                // Tạo dictionary ScrapId → Qty để lookup nhanh O(1)
                var qtyByScrap = detailQtyMap.ToDictionary(x => x.ScrapId, x => x.TotalQty);

                // Helper: tính tổng qty của 1 danh sách ScrapId
                decimal SumQty(List<Guid> ids) =>
                    ids.Sum(id => qtyByScrap.TryGetValue(id, out var qty) ? qty : 0);

                // ── B3: Lấy tên phòng ban ─────────────────────────────────────────────
                var deptIds = grouped.Select(g => g.DepartmentId).Distinct().ToList();
                var deptNames = await _departmentRepo.Query()
                    .Where(d => deptIds.Contains(d.Id))
                    .Select(d => new { d.Id, d.Name })
                    .AsNoTracking()
                    .ToListAsync();
                var deptNameMap = deptNames.ToDictionary(d => d.Id, d => d.Name);

                // ── B4: Map kết quả ───────────────────────────────────────────────────
                var result = grouped.Select(g =>
                {
                    var deptName = deptNameMap.TryGetValue(g.DepartmentId, out var n)
                        ? n : g.DepartmentId.ToString()[..8];

                    return new DepartmentReportDto
                    {
                        DepartmentId = g.DepartmentId,
                        DepartmentName = deptName,

                        TotalOrders = g.TotalOrders,
                        TotalWeight = g.TotalWeight,
                        ConfirmedWeight = g.ConfirmedWeight,

                        // ✅ Số lượng ĐÚNG theo phòng ban — chỉ sum ScrapId thuộc nhóm này
                        TotalQuantity = SumQty(g.AllScrapIds),
                        TotalQuantityApproval = SumQty(g.ApprovedScrapIds),
                        TotalQuantityReject = SumQty(g.RejectedScrapIds),
                        TotalQuantityDone = SumQty(g.DoneScrapIds),
                        TotalQuantityPending = SumQty(g.PendingScrapIds),

                        // Số đơn theo trạng thái
                        TotalOrderApproval = g.TotalOrderApproval,
                        TotalOrderReject = g.TotalOrderReject,
                        TotalOrderPending = g.TotalOrderPending,
                        TotalOrderDone = g.TotalOrderDone,
                    };
                }).ToList();

                return result;
            }
            catch (Exception ex) { throw ex; }
        }
        //public async Task<List<DepartmentReportDto>> GetDepartmentReportAsync(ReportFilterDto filter)
        //{
        //    try
        //    {
        //        var query = ApplyFilter(_scrapRepo.Query(), filter);

        //        var grouped = await query
        //            .GroupBy(x => x.DepartmentId)
        //            .Select(g => new
        //            {
        //                DepartmentId = g.Key,
        //                TotalOrders = g.Count(),
        //                TotalWeight = g.Sum(x => x.TotalAmount ?? 0),
        //                ConfirmedWeight = g.Sum(x => x.ConfirmAmount ?? 0),

        //                TotalOrderApproval = g.Count(x => x.Status == StatusScrap.Approved),
        //                TotalOrderReject = g.Count(x => x.Status == StatusScrap.Rejected),
        //                TotalOrderPending = g.Count(x => x.Status == StatusScrap.Pending),
        //                TotalOrderDone = g.Count(x => x.Status == StatusScrap.Done)

        //            })
        //            .ToListAsync();

        //        var scrapIds = await query.Select(x => x.Id).ToListAsync();
        //        var detailQty = await _detailRepo.Query()
        //            .Where(d => scrapIds.Contains(d.ScrapHistoryId))
        //            .GroupBy(d => d.ScrapHistoryId)
        //            .Select(g => new { ScrapId = g.Key, TotalQty = g.Sum(x => x.Quantity) })
        //            .ToListAsync();

        //        // Lấy tên phòng ban
        //        var deptIds = grouped.Select(g => g.DepartmentId).Distinct().ToList();
        //        var deptNames = await _departmentRepo.Query()
        //            .Where(d => deptIds.Contains(d.Id))
        //            .Select(d => new { d.Id, d.Name })
        //            .ToListAsync();

        //        var result = grouped.Select(g =>
        //        {
        //            var deptName = deptNames.FirstOrDefault(d => d.Id == g.DepartmentId)?.Name ?? g.DepartmentId.ToString()[..8];
        //            return new DepartmentReportDto
        //            {
        //                DepartmentId = g.DepartmentId,
        //                DepartmentName = deptName,
        //                TotalOrders = g.TotalOrders,
        //                TotalQuantity = detailQty.Sum(x=>x.TotalQty),
        //                TotalWeight = g.TotalWeight,
        //                ConfirmedWeight = g.ConfirmedWeight,

        //                TotalOrderApproval = g.TotalOrderApproval,
        //                TotalOrderReject = g.TotalOrderReject,
        //                TotalOrderPending = g.TotalOrderPending,
        //                TotalOrderDone = g.TotalOrderDone


        //                //StatusLabel = g.ConfirmedWeight / (g.TotalWeight == 0 ? 1 : g.TotalWeight) < 0.75m
        //                //    ? "Cần xem xét" : "Bình thường"
        //            };
        //        }).ToList();

        //        return result;
        //    }
        //    catch (Exception ex) { throw ex; }
        //}

        // ─── Material Report ──────────────────────────────────────────────────
        public async Task<List<MaterialReportDto>> GetMaterialReportAsync(ReportFilterDto filter)
        {
            try
            {
                var query = ApplyFilter(_scrapRepo.Query(), filter);

                var scrapIds = await query.Select(x => x.Id).ToListAsync();

                var confirmedScrapIds = await query
                    .Where(x => x.Status == StatusScrap.Done)
                    .Select(x => x.Id)
                    .ToListAsync();

                var details = await _detailRepo.Query()
                    .Where(d => scrapIds.Contains(d.ScrapHistoryId))
                    .ToListAsync();

                var confirmedDetails = details
                    .Where(d => confirmedScrapIds.Contains(d.ScrapHistoryId))
                    .ToList();

                var grouped = details
                    .GroupBy(d => new { d.MaterialId, d.Code, d.Name, d.TYPE, d.Unit })
                    .Select(g  => new MaterialReportDto
                    {
                        MaterialId = g.Key.MaterialId,
                        Code = g.Key.Code,
                        Name = g.Key.Name,
                        Type = g.Key.TYPE,
                        Unit = g.Key.Unit,
                        TotalQuantity = g.Sum(x => x.Quantity),
                        TotalWeight = g.Sum(x => x.TotalWeight),
                        ConfirmedWeight = confirmedDetails
                            .Where(x => x.MaterialId == g.Key.MaterialId)
                            .Sum(x => x.TotalWeight)
                    })
                    .OrderByDescending(x => x.TotalWeight).Take(5)
                    .ToList();

                return grouped;
            }
            catch (Exception ex) { throw ex; }
        }

        // ─── Trend ───────────────────────────────────────────────────────────
        public async Task<List<TrendPointDto>> GetTrendAsync(ReportFilterDto filter)
        {
            try
            {
                var baseQuery = _scrapRepo.Query();
                if (filter.FactoryId.HasValue)
                    baseQuery = baseQuery.Where(x => x.FactoryId == filter.FactoryId.Value);

                var result = new List<TrendPointDto>();

                switch (filter.Period)
                {
                    case FilterPeriod.Week:
                        {
                            // Lấy toàn bộ data của tuần 1 lần, tránh 7 round-trip DB
                            var (weekStart, weekEnd) = GetIsoWeekRange(filter.Year, filter.Week ?? 1);
                            var weekData = await baseQuery
                                .Where(x => x.CreatedDate >= weekStart && x.CreatedDate < weekEnd)
                                .ToListAsync();

                            // Chia theo từng ngày trong tuần (Thứ Hai → Chủ Nhật)
                            for (int d = 1; d <= 7; d++)
                            {
                                // ISOWeek day: 1=Monday ... 7=Sunday
                                var day = ISOWeek.ToDateTime(filter.Year, filter.Week ?? 1, (DayOfWeek)(d % 7));
                                var dayData = weekData.Where(x => x.CreatedDate.Date == day.Date).ToList();

                                result.Add(new TrendPointDto
                                {
                                    Label = day.ToString("ddd dd/MM"),
                                    TotalOrders = dayData.Count,
                                    TotalWeight = dayData.Sum(x => x.TotalAmount ?? 0),
                                    ConfirmedWeight = dayData.Sum(x => x.ConfirmAmount ?? 0)
                                });
                            }
                            break;
                        }

                    case FilterPeriod.Month:
                        {
                            // Lấy toàn bộ data của năm 1 lần
                            var yearData = await baseQuery
                                .Where(x => x.CreatedDate.Year == filter.Year)
                                .ToListAsync();

                            for (int m = 1; m <= 12; m++)
                            {
                                var mData = yearData.Where(x => x.CreatedDate.Month == m).ToList();
                                result.Add(new TrendPointDto
                                {
                                    Label = $"T{m}",
                                    TotalOrders = mData.Count,
                                    TotalWeight = mData.Sum(x => x.TotalAmount ?? 0),
                                    ConfirmedWeight = mData.Sum(x => x.ConfirmAmount ?? 0)
                                });
                            }
                            break;
                        }

                    case FilterPeriod.Year:
                        {
                            int fromYear = filter.Year - 4;
                            var rangeData = await baseQuery
                                .Where(x => x.CreatedDate.Year >= fromYear && x.CreatedDate.Year <= filter.Year)
                                .ToListAsync();

                            for (int y = fromYear; y <= filter.Year; y++)
                            {
                                var yData = rangeData.Where(x => x.CreatedDate.Year == y).ToList();
                                result.Add(new TrendPointDto
                                {
                                    Label = y.ToString(),
                                    TotalOrders = yData.Count,
                                    TotalWeight = yData.Sum(x => x.TotalAmount ?? 0),
                                    ConfirmedWeight = yData.Sum(x => x.ConfirmAmount ?? 0)
                                });
                            }
                            break;
                        }
                }

                return result;
            }
            catch (Exception ex) { throw ex; }
        }

        // ─── Approval Status ──────────────────────────────────────────────────
        public async Task<ApprovalStatusDto> GetApprovalStatusAsync(ReportFilterDto filter)
        {
            try
            {
                var query = ApplyFilter(_scrapRepo.Query(), filter);
                var scraps = await query.ToListAsync();

                return new ApprovalStatusDto
                {
                    Approved = scraps.Count(x => x.Status == StatusScrap.Approved),
                    Pending = scraps.Count(x => x.Status == StatusScrap.Pending),
                    Rejected = scraps.Count(x => x.Status == StatusScrap.Rejected)
                };
            }
            catch (Exception ex) { throw ex; }
        }

        // ─── Pipeline ─────────────────────────────────────────────────────────
        public async Task<List<ApprovalPipelineDto>> GetPipelineAsync(ReportFilterDto filter)
        {
            try
            {
                var query = ApplyFilter(_scrapRepo.Query(), filter);

                var scraps = await query
                    .Where(x => x.Status == StatusScrap.Pending)
                    .OrderByDescending(x => x.CreatedDate)
                    .Take(20)
                    .ToListAsync();

                var scrapIds = scraps.Select(x => x.Id).ToList();
                var approvals = await _approvalRepo.Query()
                    .Where(a => scrapIds.Contains(a.HistoryScrapId))
                    .ToListAsync();

                var factoryIds = scraps.Select(s => s.FactoryId).Distinct().ToList();
                var flows = await _flowRepo.Query()
                    .Where(f => factoryIds.Contains(f.FactoryId))
                    .ToListAsync();

                // Load user & department 1 lần, tránh N+1
                var decisionMakerIds = approvals
                    .Select(a => a.DecisionMaker)
                    .Distinct().ToList();
                var users = await _userRepo.Query()
                    .Where(u => decisionMakerIds.Contains(u.Id))
                    .Select(u => new { u.Id, u.FullName })
                    .ToListAsync();

                var deptIds = scraps.Select(s => s.DepartmentId).Distinct().ToList();
                var depts = await _departmentRepo.Query()
                    .Where(d => deptIds.Contains(d.Id))
                    .Select(d => new { d.Id, d.Name })
                    .ToListAsync();

                return scraps.Select(s =>
                {
                    var scrapApprovals = approvals.Where(a => a.HistoryScrapId == s.Id).ToList();
                    var currentApproval = scrapApprovals.OrderByDescending(a => a.Step).FirstOrDefault();
                    var totalSteps = flows.Count(f => f.FactoryId == s.FactoryId && f.DepartmentId == s.DepartmentId);

                    var userName = currentApproval != null
                        ? users.FirstOrDefault(u => u.Id == currentApproval.DecisionMaker)?.FullName ?? ""
                        : "";
                    var deptName = depts.FirstOrDefault(d => d.Id == s.DepartmentId)?.Name ?? "";

                    return new ApprovalPipelineDto
                    {
                        ScrapId = s.Id,
                        Title = s.Title,
                        DepartmentName = deptName,
                        DecisionMakerName = userName,
                        CreatedDate = s.CreatedDate,
                        Status = s.Status
                    };
                }).ToList();
            }
            catch (Exception ex) { throw ex; }
        }
    }
}
