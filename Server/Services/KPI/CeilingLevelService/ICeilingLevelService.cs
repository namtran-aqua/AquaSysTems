using AquaSolution.Shared.KPI.CeilingLevel;

namespace AquaSolution.Server.Services.KPi.CeilingLevelService
{
    public interface ICeilingLevelService
    {
        Task<CeilingLevelDto> CeilingLevelByUserId(Guid UserId);

    }
}
