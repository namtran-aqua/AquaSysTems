using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Data.Entities.KPI;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.KPI.DealineManagement;

namespace AquaSolution.Server.Services.KPi.FormulaService;

public class DealineManagementService : IDealineManagementService
{
    private readonly IRepository<KPIMonthlyActual> _kpiMonthlyActualRepo;
    private readonly IRepository<KPIRequest> _kpiRequestRepo;
    private readonly IRepository<KPITotalScore> _kpiTotalScoreRepo;
    private readonly IRepository<DealineKPISubmitManagement> _dealineKPISubmitManagementRepo;
    private readonly IRepository<KPIMonthlyTarget> _kpiMonthlyTargetRepo;
    public DealineManagementService(IRepository<KPIMonthlyActual> kpiMonthlyActualRepo,
        IRepository<DealineKPISubmitManagement> dealineKPISubmitManagementRepo,
         IRepository<KPIRequest> kpiRequestRepo,
         IRepository<KPITotalScore> kpiTotalScoreRepo,
    IRepository<KPIMonthlyTarget> kpiMonthlyTargetRepo)
    {
        _kpiMonthlyActualRepo = kpiMonthlyActualRepo;
        _dealineKPISubmitManagementRepo = dealineKPISubmitManagementRepo;
        _kpiMonthlyTargetRepo = kpiMonthlyTargetRepo;
        _kpiRequestRepo = kpiRequestRepo;
        _kpiTotalScoreRepo = kpiTotalScoreRepo;


    }
    public async Task<List<DealineManagementDto>> GetDealineManagement()
    {
        var now = DateTime.Now;

        var deadlines = await _dealineKPISubmitManagementRepo.GetAllAsync();
        var targets = await _kpiMonthlyTargetRepo.GetAllAsync();
        var actuals = await _kpiMonthlyActualRepo.GetAllAsync();
        var totalScores = await _kpiTotalScoreRepo.GetAllAsync();
        var requests = await _kpiRequestRepo.GetAllAsync();

        // Bước 1: lấy tất cả các tháng có target, chưa có actual, deadline hợp lệ
        var eligibleMonths = deadlines
            .Where(d =>
                d.StartDate <= now &&
                d.EndDate >= now &&
                targets.Any(t =>
                    t.Month.HasValue &&
                    t.Month.Value == d.Month &&
                    t.Year == d.Year
                ) &&
                !actuals.Any(a =>
                    a.Month.HasValue &&
                    a.Year.HasValue &&
                    a.Month.Value == d.Month &&
                    a.Year.Value == d.Year
                )
            )
            .OrderBy(d => d.Year).ThenBy(d => d.Month)
            .ToList();

        // Bước 2: kiểm tra theo thứ tự, chỉ hiển thị tháng khi tất cả các tháng trước đó đã được duyệt
        var approvedMonths = new List<DealineManagementDto>();

        foreach (var deadline in eligibleMonths)
        {
            bool allPreviousApproved = approvedMonths
                .All(prev =>
                    totalScores.Any(ts =>
                        ts.Month == prev.Month &&
                        ts.Year == prev.Year &&
                        requests.Any(r =>
                            r.SubmitId == ts.SubmitId &&
                            r.RequestStatus == StatusKPIRequestType.Approved
                        )
                    )
                );

            if (approvedMonths.Count == 0 || allPreviousApproved)
            {
                approvedMonths.Add(new DealineManagementDto
                {
                    Id = deadline.Id,
                    Month = deadline.Month,
                    Year = deadline.Year,
                    StartDate = deadline.StartDate,
                    EndDate = deadline.EndDate,
                    CreatedDate = deadline.CreatedDate
                });
            }
            else
            {
                // Dừng khi gặp tháng chưa được duyệt → không cho hiển thị các tháng sau
                break;
            }
        }

        return approvedMonths;
    }

}
