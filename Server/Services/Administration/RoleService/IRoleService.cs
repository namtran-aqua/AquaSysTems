namespace AquaSolution.Server.Services.Administration.RoleService
{
    using AquaSolution.Data.Data.Entities.Admin;
    using AquaSolution.Shared.Roles;
    using AquaSolution.Shared.UserManagements;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IRoleService
    {
        Task<List<Role>> GetAllAsync();
        Task<Role?> GetByIdAsync(Guid id);
        Task<bool> UpdateUserRolesAsync(Guid userId, List<UpdateUserRoleDto> selectedRoles);
        Task<bool> CreatedRole(HandleRoleDto handleRoleDto);
        Task<bool> DeleteAsync(Guid roleId);
    }
}
