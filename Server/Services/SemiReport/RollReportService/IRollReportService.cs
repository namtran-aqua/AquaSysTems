using AquaSolution.Shared.SemiReport;

namespace AquaSolution.Server.Services.SemiReport.RollReportService
{
    public interface IRollReportService
    {
        Task<List<RollReportDto>> GetAllAsync();
    }
}
