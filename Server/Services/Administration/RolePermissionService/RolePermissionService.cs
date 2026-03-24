using AquaSolution.Data.Data.Entities.Admin;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.RolePermissions;
using AquaSolution.Shared.UserManagements;

namespace AquaSolution.Server.Services.Administration.RolePermissionService
{
    public class RolePermissionService : IRolePermissionService
    {
        private readonly IRepository<RolePermission> _rolePermissionRepo;
        private readonly IRepository<Role> _roleRepo;
        private readonly IRepository<Permission> _permissionRepo;
        private readonly IRepository<Menu> _menuRepo;
        private readonly IRepository<Page> _pageRepo;
        public RolePermissionService(IRepository<RolePermission> rolePermissionRepo
            , IRepository<Role> roleRepo, IRepository<Permission> permissionRepo,
             IRepository<Menu> menuRepo, IRepository<Page> pageRepo
            )
        {
            _rolePermissionRepo = rolePermissionRepo;
            _roleRepo = roleRepo;
            _permissionRepo = permissionRepo;
            _menuRepo = menuRepo;
            _pageRepo = pageRepo;
        }

        public async Task<RoleDto> GetRoleWithPermissions(Guid roleId)
        {
            // Lấy Role
            var role = await _roleRepo.FirstOrDefaultAsync(r => r.Id == roleId);
            if (role == null)
                return null; // hoặc throw exception nếu muốn

            // Lấy tất cả RolePermission liên quan đến RoleId
            var rolePermissions = await _rolePermissionRepo.WhereAsync(rp => rp.RoleId == roleId);

            // Lấy tất cả Permission theo RolePermission
            var permissionIds = rolePermissions.Select(rp => rp.PermissionId).Distinct().ToList();
            var permissions = await _permissionRepo.WhereAsync(p => permissionIds.Contains(p.Id));

            // Lấy Menu và Page liên quan
            var menuIds = permissions.Where(p => p.MenuId.HasValue)
                                     .Select(p => p.MenuId!.Value)
                                     .Distinct()
                                     .ToList();
            var pageIds = permissions.Where(p => p.PageId.HasValue)
                                     .Select(p => p.PageId!.Value)
                                     .Distinct()
                                     .ToList();

            var menus = await _menuRepo.WhereAsync(m => menuIds.Contains(m.Id));
            var pages = await _pageRepo.WhereAsync(p => pageIds.Contains(p.Id));

            var menuDict = menus.ToDictionary(m => m.Id, m => m.Name);
            var pageDict = pages.ToDictionary(p => p.Id, p => p.Name);

            // Map sang RoleDto
            var roleDto = new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                IsSelected = true,
                PageId = permissions.Where(p => p.PageId.HasValue)
                                    .Select(p => p.PageId!.Value)
                                    .Distinct()
                                    .ToList()
            };

            // Map Permissions theo Menu -> Page -> Action
            foreach (var perm in permissions)
            {
                if (!perm.MenuId.HasValue || !perm.PageId.HasValue) continue;
                if (!menuDict.TryGetValue(perm.MenuId.Value, out var menuName)) continue;
                if (!pageDict.TryGetValue(perm.PageId.Value, out var pageName)) continue;

                var pageKey = $"{pageName};{perm.PageId.Value}";
                var action = perm.Action.ToString();

                if (!roleDto.Permissions.ContainsKey(menuName))
                    roleDto.Permissions[menuName] = new Dictionary<string, List<string>>();

                if (!roleDto.Permissions[menuName].ContainsKey(pageKey))
                    roleDto.Permissions[menuName][pageKey] = new List<string>();

                if (!roleDto.Permissions[menuName][pageKey].Contains(action))
                    roleDto.Permissions[menuName][pageKey].Add(action);
            }

            return roleDto;
        }


        public async Task<int> UpdateRolePermission(Guid roleId, List<Guid> SelectedPermissions)
        {
            var existing = await _rolePermissionRepo.WhereAsync(x => x.RoleId == roleId);
            if (existing != null)
            {
                _rolePermissionRepo.RemoveRange(existing);
            }
            var listRolePermission = new List<RolePermission>();
            if (SelectedPermissions.Count > 0)
            {
                foreach (var permission in SelectedPermissions)
                {
                    listRolePermission.Add(new RolePermission
                    {
                        Id = Guid.NewGuid(),
                        RoleId = roleId,
                        PermissionId = permission
                    });
                }
                await _rolePermissionRepo.InsertRangeAsync(listRolePermission);
            }
            return await _rolePermissionRepo.SaveChangesAsync();
        }
    }
}
