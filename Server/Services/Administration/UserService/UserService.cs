using AquaService.Shared.AuthModels;
using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Repositories;
using AquaSolution.Server.Services.Administration.UserService;
using AquaSolution.Shared.Administration.UserManagements;
using AquaSolution.Shared.AuthModels;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.PasswordHelpers;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

public class UserService : IUserService
{
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<UserRole> _userRoleRepo;
    private readonly IRepository<Role> _roleRepo;
    private readonly IRepository<RolePermission> _rolePermissionRepo;
    private readonly IRepository<Permission> _permissionRepo;
    private readonly IRepository<Menu> _menuRepo;
    private readonly IRepository<Page> _pageRepo;
    private readonly IRepository<Groups> _groupRepo;
    private readonly IRepository<Department> _departmentRepo;
    private readonly IRepository<Factory> _factoryRepo;
    private readonly IRepository<Position> _positionRepo;
    private readonly IConfiguration _config;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public UserService(
        IRepository<User> userRepo,
        IRepository<UserRole> userRoleRepo,
        IRepository<RolePermission> rolePermissionRepo,
        IRepository<Permission> permissionRepo,
        IRepository<Role> roleRepo,
        IHttpContextAccessor httpContextAccessor,
        IRepository<Menu> menuRepo,
        IRepository<Page> pageRepo,
        IRepository<Groups> groupRepo,
        IRepository<Department> departmentRepo,
        IRepository<Factory> factoryRepo,
        IRepository<Position> positionRepo,
        IConfiguration config)
    {
        _userRepo = userRepo;
        _userRoleRepo = userRoleRepo;
        _rolePermissionRepo = rolePermissionRepo;
        _permissionRepo = permissionRepo;
        _config = config;
        _roleRepo = roleRepo;
        _httpContextAccessor = httpContextAccessor;
        _menuRepo = menuRepo;
        _pageRepo = pageRepo;
        _groupRepo = groupRepo;
        _departmentRepo = departmentRepo;
        _factoryRepo = factoryRepo;
        _positionRepo = positionRepo;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest loginRequest)
    {
        try
        {
            var user = await _userRepo.FirstOrDefaultAsync(
                u => u.WorkDayId == loginRequest.UserName && !u.IsDeleted && u.IsActive);

            if (user == null || !PasswordHelper.VerifyPassword(user.PasswordHash, loginRequest.Password))
                return null;

            var userRoles = await _userRoleRepo
                .WhereAsync(r => r.UserId == user.Id);

            var roleIds = userRoles.Select(r => r.RoleId).ToList();

            var roles = await _roleRepo.WhereAsync(x => roleIds.Contains(x.Id));

            var rolePermissions = await _rolePermissionRepo
                .WhereAsync(rp => roleIds.Contains(rp.RoleId));
            var permissionIds = rolePermissions.Select(rp => rp.PermissionId).Distinct().ToList();
            var permissions = await _permissionRepo
                .WhereAsync(p => permissionIds.Contains(p.Id));

            var permissionNames = permissions.Select(p => p.Action.ToString()).Distinct().ToList();

            var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.FullName),
                        new Claim(ClaimTypes.Email, user.Email ?? ""),
                    };
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role.Name.ToString()));

            foreach (var permission in permissionNames)
                claims.Add(new Claim("permission", permission));

            var jwtKey = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
                throw new InvalidOperationException("JWT key not configured in appsettings.json at Jwt:Key");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddSeconds(5);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new LoginResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expires
            };
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    public async Task<UserDto?> GetCurrentUserAsync(Guid userId)
    {


        try
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            var user = await _userRepo.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return null;
            var group = await _groupRepo.FirstOrDefaultAsync(x => x.Id == user.GroupId);
            var manager = await _userRepo.FirstOrDefaultAsync(x => x.Id == user.ManagerId);
            var department = await _departmentRepo.FirstOrDefaultAsync(x => x.Id == user.DepartmentId);
            var factory = await _factoryRepo.FirstOrDefaultAsync(x => x.Id == user.FactoryId);
            var position = await _positionRepo.FirstOrDefaultAsync(x => x.Id == user.PositionId);
            var userDto = new UserDto
            {
                Id = user.Id,
                WorkDayId = user.WorkDayId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                GroupId = user.GroupId,
                GroupName = group?.Name,
                ManagerName = manager?.FullName,
                ManagerId = user.ManagerId==null?Guid.Empty: user.ManagerId.Value,
                CreatedTime = user.CreatedTime,
                IsDeleted = user.IsDeleted,
                Avatar = string.IsNullOrEmpty(user.Avatar)
                            ? $"{baseUrl}/uploads/avatars/default.jpg"
                            : $"{baseUrl}/{user.Avatar.TrimStart('/')}",
                DepartmentName = department?.Name,
                DepartmentId = user.DepartmentId,
                PositionId = user.PositionId,
                FactoryId = user.FactoryId,
                FactoryName = factory?.Name,
                PositionName = position?.Name,
                IsActive = user.IsActive,
                ManagerWorkDay =manager?.WorkDayId,
            };

            var userRoles = await _userRoleRepo.WhereAsync(ur => ur.UserId == user.Id);
            var roleIds = userRoles.Select(ur => ur.RoleId).Distinct().ToList();
            var roles = await _roleRepo.WhereAsync(r => roleIds.Contains(r.Id));

            foreach (var role in roles)
            {
                var roleDto = new RoleDto
                {
                    Id = role.Id,
                    Name = role.Name,
                    IsSelected = true
                };

                var rolePermissions = await _rolePermissionRepo
                    .WhereAsync(rp => rp.RoleId == role.Id);

                var permissionIds = rolePermissions.Select(rp => rp.PermissionId).Distinct().ToList();
                var permissions = await _permissionRepo.WhereAsync(p => permissionIds.Contains(p.Id));

                // 👉 Tách PageId
                var pageIds = permissions
                    .Where(p => p.PageId.HasValue)
                    .Select(p => p.PageId!.Value)
                    .Distinct()
                    .ToList();

                // Gán vào RoleDto nếu bạn cần dùng sau
                roleDto.PageId = pageIds;

                // Optional: xử lý phân quyền theo Menu/Page/Action như cũ
                var menuIds = permissions.Select(p => p.MenuId).Distinct().ToList();
                var menus = await _menuRepo.WhereAsync(m => menuIds.Contains(m.Id));
                var pages = await _pageRepo.WhereAsync(p => pageIds.Contains(p.Id));

                var menuDict = menus.ToDictionary(m => m.Id, m => m.Name);
                var pageDict = pages.ToDictionary(p => p.Id, p => p.Name);

                foreach (var perm in permissions)
                {
                    // Bỏ qua nếu không có MenuId hoặc PageId
                    if (!perm.MenuId.HasValue || !perm.PageId.HasValue)
                        continue;

                    // Lấy tên menu và tên page từ dictionary
                    if (!menuDict.TryGetValue(perm.MenuId.Value, out var menuName)) continue;
                    if (!pageDict.TryGetValue(perm.PageId.Value, out var pageName)) continue;

                    var pageId = perm.PageId.Value;
                    var action = perm.Action.ToString();

                    // Ghép pageName và pageId để tạo key
                    var pageKey = $"{pageName};{pageId}";

                    if (!roleDto.Permissions.ContainsKey(menuName))
                        roleDto.Permissions[menuName] = new Dictionary<string, List<string>>();

                    if (!roleDto.Permissions[menuName].ContainsKey(pageKey))
                        roleDto.Permissions[menuName][pageKey] = new List<string>();

                    if (!roleDto.Permissions[menuName][pageKey].Contains(action))
                        roleDto.Permissions[menuName][pageKey].Add(action);
                }


                userDto.Roles.Add(roleDto);
            }

            return userDto;
        }
        catch(Exception ex)
        {
            throw ex;
        }
       

    }

    public async Task<List<UserDto>> GetAllUser()
    {
        var request = _httpContextAccessor.HttpContext.Request;
        var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
        var ListUser = new List<UserDto>();
        //-------------------------
        var user = from queryableUser in await _userRepo.GetQueryableAsync()
                   join department in await _departmentRepo.GetQueryableAsync()
                   on queryableUser.DepartmentId equals department.Id
                        into d1
                   from department in d1.DefaultIfEmpty()
                   join factory in await _factoryRepo.GetQueryableAsync()
                   on queryableUser.FactoryId equals factory.Id
                    into f1
                   from factory in f1.DefaultIfEmpty()
                   join position in await _positionRepo.GetQueryableAsync()
                   on queryableUser.PositionId equals position.Id
                    into p1
                   from position in p1.DefaultIfEmpty()
                   join manager in await _userRepo.GetQueryableAsync()
                   on queryableUser.ManagerId equals manager.Id
                   into m1
                   from manager in m1.DefaultIfEmpty()
                   orderby queryableUser.CreatedTime descending
                   select new UserDto
                   {
                       Id = queryableUser.Id,
                       WorkDayId = queryableUser.WorkDayId,
                       FirstName = queryableUser.FirstName,
                       LastName = queryableUser.LastName,
                       FullName = queryableUser.FullName,
                       Email = queryableUser.Email,
                       NormalizedEmail = queryableUser.NormalizedEmail,
                       PhoneNumber = queryableUser.PhoneNumber,
                       ManagerId = queryableUser.ManagerId == null ? Guid.Empty : queryableUser.ManagerId.Value,
                       CreatedTime = queryableUser.CreatedTime,
                       UpdatedTime = queryableUser.UpdatedTime,
                       CreatedBy = queryableUser.CreatedBy,
                       UpdateBy = queryableUser.UpdateBy,
                       Avatar = string.IsNullOrEmpty(queryableUser.Avatar)
                            ? $"{baseUrl}/uploads/avatars/default.jpg"
                            : $"{baseUrl}/{queryableUser.Avatar.TrimStart('/')}",
                       DepartmentId = queryableUser.DepartmentId,
                       DepartmentName = department.Name,
                       FactoryId = queryableUser.FactoryId,
                       FactoryName = factory.Name,
                       PositionId = queryableUser.PositionId,
                       PositionName = position.Name,
                       ManagerName = manager.FullName,
                       IsActive = queryableUser.IsActive,
                       ManagerWorkDay = manager.WorkDayId
                   };
        var userlist = user.ToList();
        var userIds = userlist.Select(u => u.Id).ToList();
        var userRoles =
                    from ur in await _userRoleRepo.GetQueryableAsync()
                    join r in await _roleRepo.GetQueryableAsync()
                    on ur.RoleId equals r.Id
                    where userIds.Contains(ur.UserId)
                    select new
                    {
                        ur.UserId,
                        Role = new RoleDto
                        {
                            Id = r.Id,
                            Name = r.Name
                        }
                    };
        var listRole = userRoles.ToList();
        foreach (var userItem in userlist)
        {
            userItem.Roles = userRoles
                .Where(ur => ur.UserId == userItem.Id)
                .Select(ur => ur.Role)
                .ToList();
        }
        if (userlist.Count > 0)
        {
            return userlist;
        }
        return new List<UserDto>();
    }
    public async Task LogoutAsync()
    {
        var context = _httpContextAccessor.HttpContext;

        if (context != null)
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }

    public async Task<bool> CreatedAsync(CreatedAndUpdateUserDto createdUserDto)
    {
        try
        {
            var password = _config["DefaultUser:Password"];
            if (string.IsNullOrEmpty(password))
                throw new InvalidOperationException("Default password not configured in appsettings.json at DefaultUser:Password");

            var hashedPassword = PasswordHelper.HashPassword(password);
            var user = new User
            {
                Id = createdUserDto.Id,
                WorkDayId = createdUserDto.WorkDayId,
                FirstName = createdUserDto.FirstName,
                LastName = createdUserDto.LastName,
                FullName = createdUserDto.FullName,
                Email = createdUserDto.Email,
                PhoneNumber = createdUserDto.PhoneNumber,
                CreatedTime = createdUserDto.CreatedTime,
                ManagerId = createdUserDto.ManagerId,
                GroupId = createdUserDto.GroupId,
                PasswordHash = hashedPassword,
                NormalizedEmail = createdUserDto.Email?.ToUpper(),
                IsActive = true,
                CreatedBy = createdUserDto.CreatedBy,
                DepartmentId = createdUserDto.DepartmentId,
                PositionId = createdUserDto.PositionId,
                FactoryId = createdUserDto.FactoryId,
                Avatar = "/uploads/avatars/default.jpg"

            };
            await _userRepo.InsertAsync(user);
            await _userRepo.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            throw ex;
        }

    }
    public async Task<bool> DeleteAsync(Guid userId)
    {
        var user = await _userRepo.FirstOrDefaultAsync(x => x.Id == userId);
        if (user != null)
        {
            return await _userRepo.DeleteAsync(user);
        }
        return false;
    }
    public async Task<bool> UpdateAsync(CreatedAndUpdateUserDto updateUserDto)
    {
        try
        {
            var user = await _userRepo.FirstOrDefaultAsync(x => x.Id == updateUserDto.Id);
            if (user == null)
                return false;

            user.WorkDayId = updateUserDto.WorkDayId;
            user.FirstName = updateUserDto.FirstName;
            user.LastName = updateUserDto.LastName;
            user.FullName = updateUserDto.FullName;
            user.Email = updateUserDto.Email;
            user.PhoneNumber = updateUserDto.PhoneNumber;
            user.ManagerId = updateUserDto.ManagerId;
            user.GroupId = updateUserDto.GroupId;
            user.NormalizedEmail = updateUserDto.Email?.ToUpper();
            user.IsActive = updateUserDto.IsActive;
            user.UpdateBy = updateUserDto.UpdateBy;
            user.UpdatedTime = updateUserDto.UpdatedTime;
            user.DepartmentId = updateUserDto.DepartmentId;
            user.FactoryId = updateUserDto.FactoryId;
            user.PositionId = updateUserDto.PositionId;
            await _userRepo.UpdateAsync(user);
            return true;
        }
        catch (Exception ex)
        {
            throw ex;
        }

    }

    public async Task<bool> ChangePasswordAsync(ChangePassRequest changePassRequest)
    {
        // Step 1: Find user by Id
        var user = await _userRepo.GetByIdAsync(changePassRequest.UserId);
        if (user == null || user.IsDeleted || !user.IsActive)
            throw new Exception("User does not exist or has been deactivated.");

        // Step 2: Check if old password is correct
        var isOldPasswordCorrect = PasswordHelper.VerifyPassword(user.PasswordHash, changePassRequest.OldPassword);
        if (!isOldPasswordCorrect)
            throw new Exception("Old password is incorrect.");

        // ✅ Step 3: Validate confirm password
        if (changePassRequest.NewPassword != changePassRequest.ConfirmPassword)
            throw new Exception("Confirm password does not match the new password.");

        // Step 4: Hash the new password
        var newHashedPassword = PasswordHelper.HashPassword(changePassRequest.NewPassword);

        // Step 5: Update the new password and save changes
        user.PasswordHash = newHashedPassword;
        await _userRepo.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ChangeAvataAsync(AvataDto avataDto)
    {
        var user = await _userRepo.FirstOrDefaultAsync(x => x.Id == avataDto.UserId);
        if (user != null)
        {
            user.Avatar = avataDto.URLAvatarNew;
            return await _userRepo.UpdateAsync(user);
        }
        return false;
    }

    public async Task<List<UserContributerDto>> GetContributer()
    {
        var data = from user in await _userRepo.GetQueryableAsync()
                   join department in await _departmentRepo.GetQueryableAsync()
                   on user.DepartmentId equals department.Id
                   into d from department in d.DefaultIfEmpty()
                   select new UserContributerDto
                   {
                       Id = user.Id,
                       Name = user.FullName,
                       FactoryId =user.FactoryId,
                       DepartmentId =user.DepartmentId,
                       DepartmentType = department.DepartmentType,
                       WorkDayId = user.WorkDayId,
                       IsActive = user.IsActive,
                   };
        var listUser = data.ToList();
        if (listUser != null)
        {
            return listUser;
        }
        return new List<UserContributerDto>();
    }

    public async Task<List<UserSelectedDto>> LoadUserSelected()
    {
        var user = from userSelected in await _userRepo.GetQueryableAsync()
                   join department in await _departmentRepo.GetQueryableAsync()
                   on userSelected.DepartmentId equals department.Id
                   into d from department in d.DefaultIfEmpty()
                   join factory in await _factoryRepo.GetQueryableAsync()
                   on userSelected.FactoryId equals factory.Id
                   into f from factory in f.DefaultIfEmpty()

                   select new UserSelectedDto
                   {
                       Id = userSelected.Id,
                       Name = userSelected.FullName,
                       DepartmentId = userSelected.DepartmentId,
                       DepartmentName = department.Name,
                       WorkDayId = userSelected.WorkDayId,
                       FactoryId = userSelected.FactoryId,
                       FactoryName = factory.Name,
                       Email = userSelected.Email,

                   };
        return user.ToList();
    }
}
