using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.KPI.IndexWeight;
using AquaSolution.Shared.KPI.KPIActual;
using AquaSolution.Shared.KPI.KPISubmit;
using AquaSolution.Shared.KPI.Result;
using AquaSolution.Shared.KPI.UserTask;

namespace AquaSolution.Server.Services.KPI.KPISubmit
{
    public interface IKPISubmitService
    {
        Task<List<HandleActualDto>> GetHandleKPISubmitByUserId(Guid userId,int year, int? month);
        Task<List<HandleActualDto>> GetResultDetail(Guid userId,int year, int month);
        Task<List<YearDto>> GetKPIScoreByUserId(Guid userId, int year);
        Task<bool> SubmitKPIAsync(HandleKPISubmitDto submitKPIDto,int month);
        Task<List<KPITotalScoreDto> > GetKPITotalScoreByUserId(Guid userId, int year, int? month);
        Task<KPITotalScoreDto> GetKPITotalScoreQuarterByUserId(Guid userId, int year, int? quater);
        Task<List<IndexWeightDto>> GetIndexWeight(PositionType positionType,PeriodType periodType);
        Task<List<ViewKPITotalScoreDto>> GetKPITotalScoreByUserId(Guid userId);
        Task<List<ViewKPIForApprovalDto>> GetKPIForApproval();
        Task<List<ProcessApprovalDto>> GetProcessApprovalBySubmitIdAsync(Guid submitId);
        Task<ViewDetailApprovalKPI> GetDetailKPIBySubmitId(Guid submitId);
        Task<bool> HandleKpiForApproval(ApprovalInfo approvalInfo);
        Task<List<ViewResultKpiDto>> ResultAllKpi();
        Task<bool> CalculateQuarterPoint(List<CalculateQuarterPointDto> calculateQuarterPoint);

    }
}
