using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Menus;

namespace AquaSolution.Server.Services.MenusService
{

        public interface IMenuService
        {
            Task<List<MenuDto>> GetMenuTreeByUserId(Guid userId);
            Task<List<MenuDto>> GetAllMenuTree();
            Task<List<BaseDto>> GetAllMenu();
            Task<bool> CreatedMenu(HandleMenuDto handleMenuDto);
        }
    
}
