using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Pages;
using System.Security.Claims;

namespace AquaSolution.Server.Services.PageManagement
{
    public class PageService : IPageService
    {
        private readonly IRepository<Page> _pageRepo;
        public PageService( IRepository<Page> pageRepo )
        {
            _pageRepo = pageRepo;
        }
        public async Task<Guid> GetPageByURL(string url)
        {
            string fullPath = "/" + url.TrimStart('/'); ;
            var listPage = await _pageRepo.WhereAsync( x => x.Url.Trim() == fullPath.Trim() );
            if( listPage.Count==0)
            {
                return Guid.Empty;
            }

            var pageId = listPage.First().Id;
           return pageId;
        }

        public async Task<List<BaseDto>> GetPagesByMenu(Guid menuId)
        {
            var pages = await _pageRepo.GetQueryableAsync();
            var data = from page in pages  where page.MenuId == menuId 
                       select new BaseDto
                       {
                           Id = page.Id,
                           Name = page.Name,
                       };
            var dataReturn = data.ToList(); 

            return dataReturn;
        }
        public async Task<bool> CreatedPage(HandlePageDto handlePageDto)
        {
            var maxOrder = await _pageRepo.GetMaxAsync(m => (int?)m.Order) ?? 0;
            var page = new Page
            {
                Id = Guid.NewGuid(),
                MenuId = handlePageDto.MenuId,
                Name = handlePageDto.PageName,
                Url = "/" + handlePageDto.URL,
                Order = maxOrder + 1,
                Icon = handlePageDto.Icon
            };
            await _pageRepo.InsertAsync(page);
            return await _pageRepo.SaveChangesAsync() > 0;
        }

    }
}
