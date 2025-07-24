using AquaSolution.Data.Repositories;
using AquaSolution.Server.Services.DepartmentService;
using AquaSolution.Server.Services.MenusService;
using AquaSolution.Server.Services.PageManagement;
using AquaSolution.Server.Services.PermissionService;
using AquaSolution.Server.Services.RolePermissionService;
using AquaSolution.Server.Services.RoleService;
using AquaSolution.Server.Services.UserService;


namespace AquaSolution.Server
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            // Đăng ký UserService
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IMenuService, MenuService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IPageService, PageService>();
            services.AddScoped<IRolePermissionService, RolePermissionService>();
            services.AddScoped<IDepartmentService, DepartmentService>();

            return services;
        }
    }
}
