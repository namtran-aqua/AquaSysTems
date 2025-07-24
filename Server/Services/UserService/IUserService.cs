using AquaService.Shared.AuthModels;
using AquaSolution.Shared.AuthModels;
using AquaSolution.Shared.UserManagements;

namespace AquaSolution.Server.Services.UserService
{
    public interface IUserService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest loginRequest);
        Task<UserDto?> GetCurrentUserAsync(Guid userId);
        Task<List<UserDto?>> GetAllUser();
        Task LogoutAsync();
        Task<bool> CreatedAsync(CreatedAndUpdateUserDto createdUserDto);
        Task<bool> DeleteAsync(Guid userId);
        Task<bool> UpdateAsync(CreatedAndUpdateUserDto createdUserDto);
        Task<bool> ChangePasswordAsync(ChangePassRequest changePassRequest);
    }
}
