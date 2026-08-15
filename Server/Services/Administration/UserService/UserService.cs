using AquaService.Shared.AuthModels;
using AquaSolution.Data.Data.Entities.Admin;
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
using AquaSolution.Data.Connection;
using AquaSolution.Server.Services.Common.EmailService;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;

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
    private readonly IRepository<Section> _sectionRepo;
    private readonly IRepository<UserSection> _userSectionRepo;
    private readonly AquaDbContext _context;
    private readonly IConfiguration _config;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEmailService _emailService;
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
        IRepository<Section> sectionRepo,
        IRepository<UserSection> userSectionRepo,
        AquaDbContext context,
        IConfiguration config,
        IEmailService emailService)
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
        _sectionRepo = sectionRepo;
        _userSectionRepo = userSectionRepo;
        _context = context;
        _emailService = emailService;
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
                        new Claim(ClaimTypes.Email, user.Email ?? "")
                    };
            

            //add Claim
            var perrmission_user = await GetPermissionRole(user.Id);
            foreach (var p in perrmission_user)
            {
                claims.Add(new Claim("permission", $"{p.PageId}:{p.Action}"));
            }
            //
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role.Name.ToString()));

            foreach (var permission in permissionNames)
                claims.Add(new Claim("permission", permission));

            var jwtKey = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
                throw new InvalidOperationException("JWT key not configured in appsettings.json at Jwt:Key");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(60);
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

            var userSections = await _userSectionRepo.WhereAsync(us => us.UserId == user.Id);
            var sectionIds = userSections.Select(us => us.SectionId).ToList();
            var sections = await _sectionRepo.WhereAsync(s => sectionIds.Contains(s.Id));

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
                ManagerId = user.ManagerId == null ? Guid.Empty : user.ManagerId.Value,
                CreatedTime = user.CreatedTime,
                IsDeleted = user.IsDeleted,
                Avatar = string.IsNullOrEmpty(user.Avatar)
                            ? $"{baseUrl}/uploads/avatars/default.jpg"
                            : $"{baseUrl}/{user.Avatar.TrimStart('/')}",
                DepartmentName = department?.Name,
                DepartmentId = user.DepartmentId,

                SectionIds = sectionIds,
                Sections = sections.Select(s => new AquaSolution.Shared.Administration.Sections.SectionDto 
                            { 
                                Id = s.Id, 
                                Name = s.Name, 
                                DepartmentId = s.DepartmentId 
                            }).ToList(),
                PositionId = user.PositionId,
                FactoryId = user.FactoryId,
                FactoryName = factory?.Name,
                PositionName = position?.Name,
                IsActive = user.IsActive,
                ManagerWorkDay = manager?.WorkDayId,
                PositionType = position?.Type,
                IsChangeTask = user.IsChangeTask,
                ChangeTaskMonth = user.ChangeTaskMonth
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
        catch (Exception ex)
        {
            throw ex;
        }


    }

    public async Task<List<UserDto>> GetAllUser()
    {
        try
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            var userList = new List<UserDto>();

            // Lấy tất cả user với join các bảng liên quan
            var users = from u in await _userRepo.GetQueryableAsync()
                        join d in await _departmentRepo.GetQueryableAsync() on u.DepartmentId equals d.Id into d1
                        from department in d1.DefaultIfEmpty()


                        join f in await _factoryRepo.GetQueryableAsync() on u.FactoryId equals f.Id into f1
                        from factory in f1.DefaultIfEmpty()

                        join p in await _positionRepo.GetQueryableAsync() on u.PositionId equals p.Id into p1
                        from position in p1.DefaultIfEmpty()

                        join m in await _userRepo.GetQueryableAsync() on u.ManagerId equals m.Id into m1
                        from manager in m1.DefaultIfEmpty()

                        orderby u.CreatedTime descending
                        select new UserDto
                        {
                            Id = u.Id,
                            WorkDayId = u.WorkDayId,
                            FirstName = u.FirstName,
                            LastName = u.LastName,
                            FullName = u.FullName,
                            Email = u.Email,
                            PhoneNumber = u.PhoneNumber,
                            ManagerId = u.ManagerId == null ? Guid.Empty : u.ManagerId.Value,
                            CreatedTime = u.CreatedTime,
                            UpdatedTime = u.UpdatedTime,
                            Avatar = string.IsNullOrEmpty(u.Avatar)
                                ? null
                                : $"{baseUrl}/{u.Avatar.TrimStart('/')}",
                            DepartmentId = u.DepartmentId,
                            DepartmentName = department.Name,

                            FactoryId = u.FactoryId,
                            FactoryName = factory.Name,
                            PositionId = u.PositionId,
                            PositionName = position.Name,
                            ManagerName = manager.FullName,
                            IsActive = u.IsActive,
                            ManagerWorkDay = manager.WorkDayId,
                            PositionType = position.Type,
                            FlowApproval = u.FlowApproval,
                            IsChangeTask = u.IsChangeTask,
                            ChangeTaskMonth = u.ChangeTaskMonth,
                            Roles = new List<RoleDto>()
                        };

            var userListData = users.ToList();

            // Lấy tất cả userRole và Role
            var userIds = userListData.Select(u => u.Id).ToList();
            var allUserSections = await _userSectionRepo.WhereAsync(us => userIds.Contains(us.UserId));
            var userRoles = await _userRoleRepo.WhereAsync(ur => userIds.Contains(ur.UserId));
            var roleIds = userRoles.Select(ur => ur.RoleId).Distinct().ToList();
            var roles = await _roleRepo.WhereAsync(r => roleIds.Contains(r.Id));

            var sectionIdsForUsers = allUserSections.Select(us => us.SectionId).Distinct().ToList();
            var allSections = await _sectionRepo.WhereAsync(s => sectionIdsForUsers.Contains(s.Id));
            var sectionDict = allSections.ToDictionary(s => s.Id, s => s.Name);

            // Lấy tất cả rolePermission + Permission
            var rolePermissions = await _rolePermissionRepo.WhereAsync(rp => roleIds.Contains(rp.RoleId));
            var permissionIds = rolePermissions.Select(rp => rp.PermissionId).Distinct().ToList();
            var permissions = await _permissionRepo.WhereAsync(p => permissionIds.Contains(p.Id));
            var menuIds = permissions.Where(p => p.MenuId.HasValue).Select(p => p.MenuId!.Value).Distinct().ToList();
            var pageIds = permissions.Where(p => p.PageId.HasValue).Select(p => p.PageId!.Value).Distinct().ToList();

            var menus = await _menuRepo.WhereAsync(m => menuIds.Contains(m.Id));
            var pages = await _pageRepo.WhereAsync(p => pageIds.Contains(p.Id));

            var menuDict = menus.ToDictionary(m => m.Id, m => m.Name);
            var pageDict = pages.ToDictionary(p => p.Id, p => p.Name);

            foreach (var user in userListData)
            {
                user.SectionIds = allUserSections.Where(us => us.UserId == user.Id).Select(us => us.SectionId).ToList();
                var userSectionNames = user.SectionIds.Select(id => sectionDict.TryGetValue(id, out var name) ? name : "").Where(n => !string.IsNullOrEmpty(n)).ToList();
                user.SectionName = string.Join(", ", userSectionNames);

                var userRoleIds = userRoles.Where(ur => ur.UserId == user.Id).Select(ur => ur.RoleId).Distinct().ToList();
                var userRolesData = roles.Where(r => userRoleIds.Contains(r.Id)).ToList();

                foreach (var role in userRolesData)
                {
                    var roleDto = new RoleDto
                    {
                        Id = role.Id,
                        Name = role.Name,
                        IsSelected = true
                    };

                    var rolePerms = rolePermissions.Where(rp => rp.RoleId == role.Id).ToList();
                    var permIds = rolePerms.Select(rp => rp.PermissionId).Distinct().ToList();
                    var perms = permissions.Where(p => permIds.Contains(p.Id)).ToList();

                    // PageIds
                    roleDto.PageId = perms.Where(p => p.PageId.HasValue).Select(p => p.PageId!.Value).Distinct().ToList();

                    // Permissions theo menu/page/action
                    foreach (var perm in perms)
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

                    user.Roles.Add(roleDto);
                }
            }

            return userListData;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    //------------------
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
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var password = _config["DefaultUser:Password"];
            if (string.IsNullOrEmpty(password))
                throw new InvalidOperationException("Default password not configured in appsettings.json at DefaultUser:Password");

            var hashedPassword = PasswordHelper.HashPassword(password);
            
            if (createdUserDto.SectionIds != null && createdUserDto.SectionIds.Any())
            {
                var sections = await _sectionRepo.WhereAsync(s => createdUserDto.SectionIds.Contains(s.Id));
                if (sections.Any(s => s.DepartmentId != createdUserDto.DepartmentId))
                {
                    throw new Exception("Khu vực không hợp lệ hoặc không thuộc phòng ban đã chọn.");
                }
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                WorkDayId = createdUserDto.WorkDayId,
                FirstName = createdUserDto.FirstName,
                LastName = createdUserDto.LastName,
                FullName = createdUserDto.FullName,
                Email = createdUserDto.Email,
                PhoneNumber = createdUserDto.PhoneNumber,
                ManagerId = createdUserDto.ManagerId,
                PasswordHash = hashedPassword,
                GroupId = createdUserDto.GroupId,
                NormalizedEmail = createdUserDto.Email?.ToUpper(),
                DepartmentId = createdUserDto.DepartmentId,
                IsActive = true,
                CreatedTime = DateTime.Now,
                CreatedBy = createdUserDto.CreatedBy,
                FactoryId = createdUserDto.FactoryId,
                PositionId = createdUserDto.PositionId,
                FlowApproval = createdUserDto.FlowApproval ?? 1,
                IsChangeTask = createdUserDto.IsChangeTask,
                ChangeTaskMonth = createdUserDto.ChangeTaskMonth,
                Avatar = null
            };
            await _userRepo.InsertAsync(user);
            await _userRepo.SaveChangesAsync();

            // Insert UserSections
            if (createdUserDto.SectionIds != null && createdUserDto.SectionIds.Any())
            {
                foreach (var sId in createdUserDto.SectionIds.Distinct())
                {
                    await _userSectionRepo.InsertAsync(new UserSection
                    {
                        UserId = user.Id,
                        SectionId = sId
                    });
                }
                await _userSectionRepo.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
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
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = await _userRepo.FirstOrDefaultAsync(x => x.Id == updateUserDto.Id);
            if (user == null)
                return false;

            if (updateUserDto.SectionIds != null && updateUserDto.SectionIds.Any())
            {
                var sections = await _sectionRepo.WhereAsync(s => updateUserDto.SectionIds.Contains(s.Id));
                if (sections.Any(s => s.DepartmentId != updateUserDto.DepartmentId))
                {
                    throw new Exception("Khu vực không hợp lệ hoặc không thuộc phòng ban đã chọn.");
                }
            }

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
            user.FlowApproval = updateUserDto.FlowApproval ?? 1;
            user.IsChangeTask = updateUserDto.IsChangeTask;
            user.ChangeTaskMonth = updateUserDto.ChangeTaskMonth;
            await _userRepo.UpdateAsync(user);

            // Update UserSections
            var oldUserSections = await _userSectionRepo.WhereAsync(us => us.UserId == user.Id);
            _userSectionRepo.RemoveRange(oldUserSections);
            
            if (updateUserDto.SectionIds != null && updateUserDto.SectionIds.Any())
            {
                foreach (var sId in updateUserDto.SectionIds.Distinct())
                {
                    await _userSectionRepo.InsertAsync(new UserSection
                    {
                        UserId = user.Id,
                        SectionId = sId
                    });
                }
            }
            await _userSectionRepo.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
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
    public async Task<bool> ResetPasswordAsync(ResetPassword request)
    {
        // Step 1: Find user
        var user = await _userRepo.GetByIdAsync(request.UserId);
        if (user == null || user.IsDeleted || !user.IsActive)
            throw new Exception("User does not exist or has been deactivated.");

        // Step 2: Check confirm password
        if (request.NewPassword != request.ConfirmPassword)
            throw new Exception("Confirm password does not match the new password.");

        // Step 3: Hash new password
        var newHashedPassword = PasswordHelper.HashPassword(request.NewPassword);

        // Step 4: Update
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
                   into d
                   from department in d.DefaultIfEmpty()
                   where user.IsActive == true
                   select new UserContributerDto
                   {
                       Id = user.Id,
                       Name = user.FullName,
                       FactoryId = user.FactoryId,
                       DepartmentId = user.DepartmentId,
                       DepartmentType = department.DepartmentType,
                       WorkDayId = user.WorkDayId,
                       IsActive = user.IsActive,
                       Email = user.Email
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
                   into d
                   from department in d.DefaultIfEmpty()

                   join factory in await _factoryRepo.GetQueryableAsync()
                   on userSelected.FactoryId equals factory.Id
                   into f
                   from factory in f.DefaultIfEmpty()

                   where userSelected.IsActive == true

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
    private async Task<List<UserPermissionDto>> GetPermissionRole(Guid userId)
    {
        var permission_User =
        from ur in await _userRoleRepo.GetQueryableAsync()
        join rp in await _rolePermissionRepo.GetQueryableAsync() on ur.RoleId equals rp.RoleId
        join p in await _permissionRepo.GetQueryableAsync() on rp.PermissionId equals p.Id
        join page in await _pageRepo.GetQueryableAsync() on p.PageId equals page.Id
        where ur.UserId == userId
        select new UserPermissionDto
        {
            PageId = page.Id,
            PageName = page.Name,
            Action = p.Action
        };
        return permission_User.ToList();
    }

    public async Task<bool> SendOtpAsync(ForgotPasswordRequest request)
    {
        var user = await _userRepo.FirstOrDefaultAsync(u => u.WorkDayId == request.WorkDayId && u.IsActive && !u.IsDeleted);
        if (user == null)
        {
            throw new Exception("WorkDay ID không tồn tại hoặc đã bị vô hiệu hoá.");
        }

        if (string.IsNullOrEmpty(user.Email) || !user.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception("WorkDay ID và Email không trùng khớp.");
        }

        using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        await connection.OpenAsync();

        // Invalidate old OTPs
        var invalidateSql = "UPDATE Admin.Tbl_PasswordReset SET IsUsed = 1 WHERE UserID = @UserId AND IsUsed = 0";
        await connection.ExecuteAsync(invalidateSql, new { UserId = user.Id });

        // Generate 6-digit OTP
        var otp = new Random().Next(100000, 999999).ToString();
        var otpHash = PasswordHelper.HashPassword(otp);

        var insertSql = @"
            INSERT INTO Admin.Tbl_PasswordReset (UserID, OtpHash, Attempts, OtpExpiredAt, IsUsed, CreatedDate)
            VALUES (@UserId, @OtpHash, 0, @OtpExpiredAt, 0, GETDATE())";

        await connection.ExecuteAsync(insertSql, new
        {
            UserId = user.Id,
            OtpHash = otpHash,
            OtpExpiredAt = DateTime.Now.AddMinutes(5)
        });

        // Send email
        await _emailService.SendPasswordResetOtpAsync(user.Email, user.WorkDayId, otp);

        return true;
    }

    public async Task<string> VerifyOtpAsync(VerifyOtpRequest request)
    {
        var user = await _userRepo.FirstOrDefaultAsync(u => u.WorkDayId == request.WorkDayId && u.IsActive && !u.IsDeleted);
        if (user == null)
            throw new Exception("Invalid request.");

        using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        await connection.OpenAsync();

        var selectSql = @"
            SELECT TOP 1 ID, UserID, OtpHash, Attempts, OtpExpiredAt, IsUsed
            FROM Admin.Tbl_PasswordReset
            WHERE UserID = @UserId AND IsUsed = 0 AND OtpExpiredAt > GETDATE()
            ORDER BY CreatedDate DESC";

        var resetRecord = await connection.QueryFirstOrDefaultAsync<dynamic>(selectSql, new { UserId = user.Id });
        if (resetRecord == null)
            throw new Exception("OTP expired or invalid.");

        if (resetRecord.Attempts >= 5)
        {
            await connection.ExecuteAsync("UPDATE Admin.Tbl_PasswordReset SET IsUsed = 1 WHERE ID = @Id", new { Id = resetRecord.ID });
            throw new Exception("Too many attempts. Please request a new OTP.");
        }

        if (!PasswordHelper.VerifyPassword((string)resetRecord.OtpHash, request.Otp))
        {
            await connection.ExecuteAsync("UPDATE Admin.Tbl_PasswordReset SET Attempts = Attempts + 1 WHERE ID = @Id", new { Id = resetRecord.ID });
            throw new Exception("Invalid OTP.");
        }

        // OTP is correct. Generate ResetToken.
        var resetTokenBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(resetTokenBytes);
        }
        var resetToken = Convert.ToBase64String(resetTokenBytes);
        var resetTokenHash = PasswordHelper.HashPassword(resetToken);

        var updateSql = @"
            UPDATE Admin.Tbl_PasswordReset
            SET ResetToken = @ResetTokenHash,
                ResetTokenExpiredAt = DATEADD(minute, 15, GETDATE())
            WHERE ID = @Id";

        await connection.ExecuteAsync(updateSql, new { ResetTokenHash = resetTokenHash, Id = resetRecord.ID });

        return resetToken;
    }

    public async Task<bool> ResetPasswordWithTokenAsync(ResetPasswordRequest request)
    {
        if (request.NewPassword != request.ConfirmPassword)
            throw new Exception("Confirm password does not match the new password.");

        using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        await connection.OpenAsync();

        var selectSql = @"
            SELECT TOP 1 ID, UserID, ResetToken, ResetTokenExpiredAt, IsUsed
            FROM Admin.Tbl_PasswordReset
            WHERE IsUsed = 0 AND ResetTokenExpiredAt > GETDATE()
            ORDER BY CreatedDate DESC";

        var resetRecords = await connection.QueryAsync<dynamic>(selectSql);
        dynamic validRecord = null;
        foreach (var record in resetRecords)
        {
            if (record.ResetToken != null && PasswordHelper.VerifyPassword((string)record.ResetToken, request.ResetToken))
            {
                validRecord = record;
                break;
            }
        }

        if (validRecord == null)
            throw new Exception("Invalid or expired reset token.");

        using var transaction = await connection.BeginTransactionAsync();
        try
        {
            var user = await _userRepo.GetByIdAsync((Guid)validRecord.UserID);
            if (user == null || user.IsDeleted || !user.IsActive)
                throw new Exception("User does not exist or has been deactivated.");

            var newHashedPassword = PasswordHelper.HashPassword(request.NewPassword);
            user.PasswordHash = newHashedPassword;

            // In EF Core, we should use the DbContext for updating user so it tracks it or just update it via _userRepo
            await _userRepo.UpdateAsync(user);

            var updateTokenSql = "UPDATE Admin.Tbl_PasswordReset SET IsUsed = 1 WHERE ID = @Id";
            await connection.ExecuteAsync(updateTokenSql, new { Id = validRecord.ID }, transaction);

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
