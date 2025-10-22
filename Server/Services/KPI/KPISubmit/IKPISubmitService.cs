using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.KPI.KPIActual;
using AquaSolution.Shared.KPI.KPISubmit;

namespace AquaSolution.Server.Services.KPI.KPISubmit
{
    public interface IKPISubmitService
    {
        Task<List<HandleKPISubmitDto>> GetHandleKPISubmitByUserId(Guid userId,int year, int? month);
        Task<List<YearDto>> GetKPIScoreByUserId(Guid userId, int year);
        Task<bool> SubmitKPIAsync(List<HandleKPISubmitDto> submitKPIDto);
        Task<List<KPITotalScoreDto> > GetKPITotalScoreByUserId(Guid userId, int year, int? month);
        Task<List<KPITotalScoreDto>> GetKPITotalScoreQuarterByUserId(Guid userId, int year, int? quater);
        Task<List<IndexWeightDto>> GetIndexWeight(PositionType positionType,PeriodType periodType);
    }
}
