using AquaSolution.Data.Data.Entities.Admin;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.Roles;

namespace AquaSolution.Server.Services.Administration.RoleService
{
    public class RoleService : IRoleService
    {

        private readonly IRepository<Role> _roleRepository;
        private readonly IRepository<UserRole> _userRoleRepository;
        public RoleService(IRepository<Role> roleRepository, IRepository<UserRole> userRoleRepository)
        {
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
        }
        public async Task<List<Role>> GetAllAsync()
        {
            return await _roleRepository.GetAllAsync();
        }

        public async Task<Role?> GetByIdAsync(Guid id)
        {
            return await _roleRepository.FirstOrDefaultAsync(r => r.Id == id);
        }
        public async Task<bool> UpdateUserRolesAsync(Guid userId, List<UpdateUserRoleDto> selectedRoleIds)
        {
            var existingUserRoles = await _userRoleRepository.WhereAsync(ur => ur.UserId == userId);
            if (existingUserRoles.Any())
            {
                _userRoleRepository.RemoveRange(existingUserRoles);
            }
            var newUserRoles = selectedRoleIds.Select(roleId => new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RoleId = roleId.Id
            }).ToList();
            await _userRoleRepository.InsertRangeAsync(newUserRoles);
            await _userRoleRepository.SaveChangesAsync(); 
            return true;
        }

        public async Task<bool> CreatedRole(HandleRoleDto handleRoleDto)
        {
            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = handleRoleDto.RoleName
            };
            await _roleRepository.InsertAsync(role);
            return await _roleRepository.SaveChangesAsync() ==1;
        }

        public async Task<bool> DeleteAsync(Guid roleId)
        {
            var role = await _roleRepository.FirstOrDefaultAsync(x => x.Id == roleId);
           if(role == null)
            {
                return false;
            }
            return await _roleRepository.DeleteAsync(role);
        }
    }
}
