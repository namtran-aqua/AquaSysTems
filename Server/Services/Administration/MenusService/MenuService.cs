using AquaSolution.Data.Data.Entities.Admin;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Menus;
using AquaSolution.Shared.Pages;

namespace AquaSolution.Server.Services.Administration.MenusService
{
    public class MenuService : IMenuService
    {
        private readonly IRepository<UserRole> _userRoleRepo;
        private readonly IRepository<RolePermission> _rolePermissionRepo;
        private readonly IRepository<Permission> _permissionRepo;
        private readonly IRepository<Menu> _menuRepo;
        private readonly IRepository<Page> _pageRepo;
        public MenuService(
                IRepository<UserRole> userRoleRepo,
                IRepository<RolePermission> rolePermissionRepo,
                IRepository<Permission> permissionRepo,
                IRepository<Menu> menuRepo,
                IRepository<Page> pageRepo)
        {
            _userRoleRepo = userRoleRepo;
            _rolePermissionRepo = rolePermissionRepo;
            _permissionRepo = permissionRepo;
            _menuRepo = menuRepo;
            _pageRepo = pageRepo;
        }
        public async Task<List<MenuDto>> GetMenuTreeByUserId(Guid userId)
        {
            // Step 1: Lấy các RoleId của user
            var roleIds = (await _userRoleRepo.FindAsync(ur => ur.UserId == userId))
                            .Select(ur => ur.RoleId)
                            .Distinct()
                            .ToList();

            // Step 2: Lấy các PermissionId theo Role
            var permissionIds = (await _rolePermissionRepo
                .FindAsync(rp => roleIds.Contains(rp.RoleId)))
                .Select(rp => rp.PermissionId)
                .Distinct()
                .ToList();

            // Step 3: Lấy các permission liên quan
            var permissions = (await _permissionRepo
                .FindAsync(p => permissionIds.Contains(p.Id)))
                .ToList();

            var menuIds = permissions
                .Where(p => p.MenuId.HasValue)
                .Select(p => p.MenuId!.Value)
                .Distinct()
                .ToList();

            var pageIds = permissions
                .Where(p => p.PageId.HasValue)
                .Select(p => p.PageId!.Value)
                .Distinct()
                .ToList();

            var menus = (await _menuRepo
                .FindAsync(m => menuIds.Contains(m.Id)))
                .OrderBy(m => m.Order)
                .ToList();

            var pages = (await _pageRepo
                .FindAsync(p => pageIds.Contains(p.Id)))
                .ToList();

            var result = menus.Select(menu => new MenuDto
            {
                Id = menu.Id,
                Name = menu.Name,
                Icon = menu.Icon,
                Order = menu.Order,
                Pages = pages
                    .Where(p => p.MenuId == menu.Id)
                    .Select(p => new PageDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Url = p.Url,
                        Order = p.Order,
                        Icon = p.Icon,
                        DesCription = p.Description,
                        Permissions = permissions
                            .Where(per => per.PageId == p.Id)
                            .Select(per => per.Action.ToString())
                            .ToList()
                    })
                    .ToList()
            }).ToList();

            return result;
        }
        public async Task<List<MenuDto>> GetAllMenuTree()
        {
            // Bước 1: Lấy tất cả permission
            var permissions = await _permissionRepo.GetQueryableAsync();

            // Bước 2: Lấy tất cả pageId có permission
            var validPages = (await _pageRepo.GetAllAsync())
               .ToList();
            // Bước 4: Lấy toàn bộ menu
            var allMenus = (await _menuRepo
                .GetAllAsync())
                .OrderBy(m => m.Order)
                .ToList();

            // Bước 5: Tạo kết quả
            var result = allMenus.Select(menu => new MenuDto
            {
                Id = menu.Id,
                Name = menu.Name,
                Icon = menu.Icon,
                Order = menu.Order,
                Pages = validPages
                    .Where(p => p.MenuId == menu.Id)
                    .Select(p => new PageDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Url = p.Url,
                        Order = p.Order,
                        Icon = p.Icon,
                        DesCription = p.Description,
                        Permissions = permissions
                            .Where(per => per.PageId == p.Id)
                            .Select(per => per.Action.ToString())
                            .ToList(),
                        JoinPermissions = string.Join("-", permissions
                            .Where(per => per.PageId == p.Id)
                            .Select(per => per.Action.ToString()))
                    })
                    .ToList()
            }).ToList();
            return result;
        }
        public async Task<List<BaseDto>> GetAllMenu()
        {
            var result = new List<BaseDto>();
            var menus = from menu in await _menuRepo.GetQueryableAsync()
                        select new BaseDto
                        {
                            Id = menu.Id,
                            Name = menu.Name,
                        };
            return menus.ToList();
        }
        public async Task<bool> CreatedMenu(HandleMenuDto handleMenuDto)
        {
            var maxOrder = await _menuRepo.GetMaxAsync(m => (int?)m.Order) ?? 0;
            var menu = new Menu
            {
                Order = maxOrder+1,
                Id = Guid.NewGuid(),
                Name = handleMenuDto.Name,
                Icon = handleMenuDto.Icon == null ? string.Empty : handleMenuDto.Icon,
            };
            var permission = new Permission
            {
                Id = Guid.NewGuid(),
                Action = handleMenuDto.Action,
                Type = handleMenuDto.Type,
                MenuId = menu.Id,
                PageId = null
            };
            await _permissionRepo.InsertAsync(permission);
            await _menuRepo.InsertAsync(menu);
            return await _menuRepo.SaveChangesAsync() > 0;
        }
    }
}
