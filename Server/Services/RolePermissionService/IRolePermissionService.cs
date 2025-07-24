namespace AquaSolution.Server.Services.RolePermissionService
{
    public interface IRolePermissionService
    {
        Task<int> UpdateRolePermission(Guid roleId, List<Guid> SelectedPermissions);
    }
}
