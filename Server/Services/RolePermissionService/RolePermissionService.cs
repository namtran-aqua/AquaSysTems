
using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.RolePermissions;

namespace AquaSolution.Server.Services.RolePermissionService
{
    public class RolePermissionService : IRolePermissionService
    {
        private readonly IRepository<RolePermission> _rolePermissionRepo;
        public RolePermissionService(IRepository<RolePermission> rolePermissionRepo)
        {
            _rolePermissionRepo = rolePermissionRepo;
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
                await _rolePermissionRepo.AddRangeAsync(listRolePermission);
            }
            return await _rolePermissionRepo.SaveChangesAsync();
        }
    }
}
