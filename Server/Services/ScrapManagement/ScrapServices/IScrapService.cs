using AquaSolution.Shared.ScrapManagement.Scrap;

namespace AquaSolution.Server.Services.ScrapManagetment.ScapServices
{
    // Interface thêm method
    public interface IScrapService
    {
        Task<List<HistoryScrapDto>> GetHistory();
        Task CreateScrap(HandleScrapDto createScrapDto);
        Task<List<HistoryScrapDto>> GetScrapForApproval(Guid userId);
        Task ActionApproval(ApprovalActionDto request);
        Task ConfirmScrap(ConfirmScrapDto request); 
        Task<List<HistoryScrapDto>> GetScrapForConfirm(); 
    }
}