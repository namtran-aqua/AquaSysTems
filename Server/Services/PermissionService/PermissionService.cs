using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Menus;
using AquaSolution.Shared.Pages;
using AquaSolution.Shared.Permissions;

namespace AquaSolution.Server.Services.PermissionService
{
    public class PermissionService : IPermissionService
    {
        private readonly IRepository<Permission> _permissionRepo;
        private readonly IRepository<RolePermission> _rolePermissionRepo;
        private readonly IRepository<Menu> _menuRepo;
        private readonly IRepository<Page> _pageRepo;

        public PermissionService(IRepository<Permission> permissionRepo,
            IRepository<RolePermission> rolePermissionRepo,
            IRepository<Menu> menuRepo,
            IRepository<Page> pageRepo
            )
        {
            _permissionRepo = permissionRepo;
            _rolePermissionRepo = rolePermissionRepo;
            _menuRepo = menuRepo;
            _pageRepo = pageRepo;
        }


        public async Task<List<GroupedPermissionDto>> GetGroupedPermissionsByMenuViewAsync()
        {
            var allPermissions = await _permissionRepo.GetAllAsync();
            var allMenus = await _menuRepo.GetAllAsync();
            var allPages = await _pageRepo.GetAllAsync();

            var groupedResult = new List<GroupedPermissionDto>();

            // Lọc các permission loại Menu có Action = View
            var menuPermissions = allPermissions
                .Where(p => p.Type == PermissionType.Menu && p.Action.ToString() == "View" && p.MenuId.HasValue)
                .ToList();

            foreach (var menuPermission in menuPermissions)
            {
                var menuId = menuPermission.MenuId!.Value;
                var menu = allMenus.FirstOrDefault(m => m.Id == menuId);
                if (menu == null) continue;
                var relatedPermissions = allPermissions
                    .Where(p => p.MenuId == menuId)
                    .Select(p =>
                    {
                        var pageName = p.PageId.HasValue
                            ? allPages.FirstOrDefault(pg => pg.Id == p.PageId.Value)?.Name ?? ""
                            : "";

                        return new PermissionDto
                        {
                            PermissionId = p.Id,
                            MenuId = p.MenuId,
                            MenuName = menu.Name,
                            PageId = p.PageId,
                            PageName = pageName,
                            Action = p.Action.ToString()
                        };
                    })
                    .ToList();

                groupedResult.Add(new GroupedPermissionDto
                {
                    PermissionId = menuPermission.Id,
                    MenuId = menuId,
                    MenuName = menu.Name,
                    Permissions = relatedPermissions
                });
            }
            return groupedResult;
        }
        public async Task<List<GroupedPermissionDto>> GetPermissionTreeByRoleId(Guid roleId)
        {
            var rolePermissionIds = (await _rolePermissionRepo
                .WhereAsync(rp => rp.RoleId == roleId))
                .Select(rp => rp.PermissionId)
                .ToList();

            var allPermissions = await _permissionRepo.GetAllAsync();
            var allMenus = await _menuRepo.GetAllAsync();
            var allPages = await _pageRepo.GetAllAsync();

            var groupedResult = new List<GroupedPermissionDto>();

            // Lọc permission thuộc role + là loại MENU (Action = "View") + có MenuId
            var menuPermissions = allPermissions
                .Where(p => rolePermissionIds.Contains(p.Id)
                    && p.Type == PermissionType.Menu
                    && p.Action.ToString() == "View"
                    && p.MenuId.HasValue)
                .ToList();

            foreach (var menuPermission in menuPermissions)
            {
                var menuId = menuPermission.MenuId!.Value;
                var menu = allMenus.FirstOrDefault(m => m.Id == menuId);
                if (menu == null) continue;

                var relatedPermissions = allPermissions
                    .Where(p => p.MenuId == menuId && rolePermissionIds.Contains(p.Id))
                    .Select(p =>
                    {
                        var pageName = p.PageId.HasValue
                            ? allPages.FirstOrDefault(pg => pg.Id == p.PageId.Value)?.Name ?? ""
                            : "";

                        return new PermissionDto
                        {
                            PermissionId = p.Id,
                            MenuId = p.MenuId,
                            MenuName = menu.Name,
                            PageId = p.PageId,
                            PageName = pageName,
                            Action = p.Action.ToString()
                        };
                    })
                    .ToList();

                groupedResult.Add(new GroupedPermissionDto
                {
                    PermissionId = menuPermission.Id,
                    MenuId = menuId,
                    MenuName = menu.Name,
                    Permissions = relatedPermissions
                });
            }

            return groupedResult;
        }
        public async Task<List<MenuDto>> GetPermissionAll()
        {
            try
            {
                var menus = await _menuRepo.GetAllAsync();
                var pages = await _pageRepo.GetAllAsync();
                var permissions = await _permissionRepo
                    .WhereAsync(p => p.Type == PermissionType.Page); // Chỉ lấy permission theo Page

                var menuDtos = menus
                    .OrderBy(m => m.Order)
                    .Select(menu =>
                    {
                        var menuPages = pages
                            .Where(p => p.MenuId == menu.Id)
                            .OrderBy(p => p.Order)
                            .Select(page =>
                            {
                                var pagePermissions = permissions
                                    .Where(p => p.PageId == page.Id)
                                    .Select(p => new HandlePermissionDto
                                    {
                                        Id = p.Id,
                                        Type = p.Type,
                                        Action = p.Action,
                                        PageId = p.PageId,
                                        MenuId = null // luôn null
                                    })
                                    .ToList();

                                return new PageDto
                                {
                                    Id = page.Id,
                                    Name = page.Name,
                                    Url = page.Url,
                                    Permissions = pagePermissions.Select(x => x.Action.ToString()).ToList(),
                                    JoinPermissions = string.Join(", ", pagePermissions.Select(x => x.Action.ToString())),
                                    HandlePermissionDtos = pagePermissions
                                };
                            })
                            .ToList();

                        return new MenuDto
                        {
                            Id = menu.Id,
                            Name = menu.Name,
                            Icon = menu.Icon,
                            Order = menu.Order,
                            Pages = menuPages
                        };
                    })
                    .ToList();

                return menuDtos;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> CreatedPermission(HandlePermissionDto HandlePermissionDto)
        {
            var permision = new Permission
            {
                Id = Guid.NewGuid(),
                Action = HandlePermissionDto.Action,
                Type = HandlePermissionDto.Type,
                MenuId = HandlePermissionDto.MenuId,
                PageId = HandlePermissionDto.PageId,
            };
            await _permissionRepo.InsertAsync(permision);
            return await _permissionRepo.SaveChangesAsync() ==1;
        }
        public async Task<bool> DeletePermission(Guid permissionId)
        {
            var permission = await _permissionRepo.FirstOrDefaultAsync(x => x.Id == permissionId);
            if(permission == null)
            {
                return false;
            }
            return await _permissionRepo.DeleteAsync(permission);
        }

    }
}
