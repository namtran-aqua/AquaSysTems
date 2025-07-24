using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Pages;

namespace AquaSolution.Server.Services.PageManagement
{
    public interface IPageService
    {
        Task<Guid>GetPageByURL(string url);
        Task<List<BaseDto>>GetPagesByMenu(Guid menuId);
        Task<bool>CreatedPage(HandlePageDto handlePageDto);

    }
}
