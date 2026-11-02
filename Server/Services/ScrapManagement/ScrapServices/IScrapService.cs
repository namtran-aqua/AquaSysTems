using AquaSolution.Shared.ScrapManagement.Scrap;

namespace AquaSolution.Server.Services.ScrapManagetment.ScapServices
{
    // Interface thêm method
    public interface IScrapService
    {
        Task<List<HistoryScrapDto>> GetHistory();
        Task CreateScrap(HandleScrapDto createScrapDto);
        Task<List<HistoryScrapDto>> GetScrapForApproval(Guid userId, bool isAdmin);
        Task ActionApproval(ApprovalActionDto request);
        Task ConfirmScrap(ActionConfirmScrapDto request); 
        Task<List<HistoryScrapDto>> GetScrapForConfirm(); 
        Task<List<WasteItemDto>> GetAllWasteItemsAsync();
        Task<List<RequestApprovalDto>> PreviewApprovalFlow(Guid factoryId, Guid departmentId, Guid? sectionId, Guid creatorId);
        Task<List<AquaSolution.Shared.ScrapManagement.Receivers.ScrapReceiverDto>> GetScrapReceivers();
        Task SaveScrapReceiver(AquaSolution.Shared.ScrapManagement.Receivers.SaveScrapReceiverDto dto);
        Task DeleteScrapReceiver(Guid id);
        Task ResubmitScrap(HandleScrapDto resubmitDto);
    }
}