using AquaSolution.Shared.SemiReport;

namespace AquaSolution.Server.Services.SemiReport.CusPackService
{
    public interface ICusPackService
    {
        Task<bool> CreatedAsync(CusPackNoDto cusPackDto);
        Task<bool> DeleteAsync(int cusPackId);
        Task<bool> UpdateAsync(CusPackNoDto cusPackDto);
        Task<List<CusPackNoDto>> GetAllAsync();
    }
}
