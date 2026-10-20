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
       
        Task<List<YearDto>> GetKPIScoreByUserId(Guid userId, int year);
        Task<bool> SubmitKPIAsync(HandleKPISubmitDto submitKPIDto,int month);

        Task<List<IndexWeightDto>> GetIndexWeight(PositionType positionType,PeriodType periodType);
        Task<List<ViewKPITotalScoreDto>> GetKPITotalScoreByUserId(Guid userId);
        Task<List<ViewKPIForApprovalDto>> GetKPIForApproval();
        Task<List<ProcessApprovalDto>> GetProcessApprovalBySubmitIdAsync(Guid submitId);
        Task<ViewDetailApprovalKPI> GetDetailKPIBySubmitId(Guid submitId);
        Task<bool> HandleKpiForApproval(ApprovalInfo approvalInfo);
        Task<List<ViewResultKpiDto>> ResultAllKpi();
        Task<bool> CalculateQuarterPoint(HandleKPISubmitDto calculateQuarterPoint);
        #region GET RESULT CALCULATED
        Task<List<KPITotalScoreDto>> GetKPITotalScoreByUserId(Guid userId, int year, int month);
        Task<List<HandleActualDto>> GetResultDetail(Guid userId, int year, int month);
        Task<List<HandleActualDto>> GetResultDetailByQuarter(Guid userId, int year, int quater);
        Task<KPITotalScoreDto> GetResultTotalByQuarter(Guid userId, int year, int quater);
        Task<YearPrecheckResultDto> CheckYearEligibilityAsync(Guid userId, int year);
        Task<SpecialPeriodYearResultDto> CheckSpecialPeriodYearEligibilityAsync(Guid userId, int year);
        Task<bool> SubmitSpecialPeriodYearAsync(SubmitSpecialPeriodYearDto dto, Guid currentUserId);
        #endregion


    }
}
