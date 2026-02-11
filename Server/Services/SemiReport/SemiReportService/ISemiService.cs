using AquaSolution.Shared.SemiReport;

namespace AquaSolution.Server.Services.SemiReport.SemiReportService
{
    public interface ISemiService
    {
        Task<List<SemiReportDto>> GetAllAsync();
        Task<bool> GetInner4HourStatusAsync();
        Task<bool> UpdateInner4HourStatusAsync(bool isActive);
    }
}
