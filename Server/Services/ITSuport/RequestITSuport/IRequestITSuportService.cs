using AquaSolution.Shared.ITSuport.Attachments;
using AquaSolution.Shared.ITSuport.RequestSuport;
using System.Net.Mail;

namespace AquaSolution.Server.Services.ITSuport.RequestSuportCategories
{
    public interface IRequestITSuportService
    {
        Task<bool> CreatedAsync(HandleRequestSuportDto handleRequestSuportDto);
        Task<bool> DeleteAssync(Guid requestITSuportId);
        Task<bool> UpdateAsync(HandleRequestSuportDto handleRequestSuportDto);
        Task<List<AttachmentDto>> LoadListAttachment(Guid requestITSuportId);
        Task<List<RequestSuportDto>> GetAllAsync();
        Task<PagedResult<RequestSuportDto>> GetPagedAsync(RequestSuportQueryDto request);
        Task<RequestSuportStatsDto> GetStatsAsync(Guid? currentUserId, bool isITOrAdmin);
    }
}
