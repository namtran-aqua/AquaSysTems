using AquaSolution.Data.Repositories;
using AquaSolution.Server.Services.Administration.ApprovalFlowService;
using AquaSolution.Server.Services.Administration.DepartmentService;
using AquaSolution.Server.Services.Administration.FactoryService;
using AquaSolution.Server.Services.Administration.MenusService;
using AquaSolution.Server.Services.Administration.PageManagement;
using AquaSolution.Server.Services.Administration.PermissionService;
using AquaSolution.Server.Services.Administration.PositionService;
using AquaSolution.Server.Services.Administration.RolePermissionService;
using AquaSolution.Server.Services.Administration.RoleService;
using AquaSolution.Server.Services.Administration.SystemLock;
using AquaSolution.Server.Services.Administration.UserService;
using AquaSolution.Server.Services.Common.HandleInventories;
using AquaSolution.Server.Services.Common.UserConnectionManager;
using AquaSolution.Server.Services.ePAD;
using AquaSolution.Server.Services.HRMS;
using AquaSolution.Server.Services.ITSuport.RequestSuportCategories;
using AquaSolution.Server.Services.KPi.FormulaService;
using AquaSolution.Server.Services.KPi.KPITaskService;
using AquaSolution.Server.Services.KPi.QuarterCalculateds;
using AquaSolution.Server.Services.KPI.IndexWeight;
using AquaSolution.Server.Services.KPI.KPISubmit;
using AquaSolution.Server.Services.KPI.KPIUserTask;
using AquaSolution.Server.Services.ManageMedicalRooms.InventoriesService;
using AquaSolution.Server.Services.ManageMedicalRooms.InventoryPeriodService;
using AquaSolution.Server.Services.ManageMedicalRooms.MedicineSupplyRequestService;
using AquaSolution.Server.Services.ManageMedicalRooms.Products;
using AquaSolution.Server.Services.ManageMedicalRooms.RequestClinicservice;
using AquaSolution.Server.Services.ManageMedicalRooms.WarehouseExportService;
using AquaSolution.Server.Services.ManageMedicalRooms.WarehouseImportService;
using AquaSolution.Server.Services.SemiReport.CusPackService;
using AquaSolution.Server.Services.SemiReport.RollReportService;
using AquaSolution.Server.Services.SemiReport.SemiReportService;


namespace AquaSolution.Server
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            // Đăng ký UserService
            #region Admin
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IMenuService, MenuService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IPageService, PageService>();
            services.AddScoped<IRolePermissionService, RolePermissionService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IFactoryService, FactoryService>();
            services.AddScoped<IPositionService, PositionService>();
            services.AddScoped<IApprovalFlowService, ApprovalFlowService>();
            services.AddScoped<ISystemLockService, SystemLockService>();

            #endregion
            #region Medical
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IInventoryService, InventoryService>();
            services.AddScoped<IWarehouseImportService, WarehouseImportService>();
            services.AddScoped<IWarehouseExportService, WarehouseExportService>();
            services.AddScoped<IRequestClinicservice, RequestClinicservice>();
            services.AddScoped<IHandleInventory, HandleInventory>();
            services.AddScoped<IInventoryPeriodService, InventoryPeriodService>();
            services.AddScoped<IMedicineSupplyRequestService, MedicineSupplyRequestService>();
            #endregion
            #region IT Suport
            services.AddScoped<IRequestSuportCategoryService, RequestSuportCategoryService>();
            services.AddScoped<IRequestITSuportService, RequestITSuportService>();
            #endregion
            #region KPI
            services.AddScoped<IFormulaService, FormulaService>();
            services.AddScoped<IKPITaskService, KPITaskService>();
            services.AddScoped<IUserTaskService, UserTaskService>();
            services.AddScoped<IKPIMonthlyTargetService, KPIMonthlyTargetService>();
            services.AddScoped<IKPISubmitService, KPISubmitService>();
            services.AddScoped<IDealineManagementService, DealineManagementService>();
            services.AddScoped<IQuarterCalculatedService, QuarterCalculatedService>();
            services.AddScoped<IIndexWeightService, IndexWeightService>();

            #endregion
            #region Common
            services.AddScoped<IUserConnectionManager, UserConnectionManager>();
            #endregion
            #region ePAD
            services.AddScoped<IePADService, ePADService>();
            #endregion
            #region HRMS
            services.AddScoped<IHRMSService, HRMSService>();
            #endregion
            #region SemiReport
            services.AddScoped<ISemiService, SemiService>();
            services.AddScoped<IRollReportService, RollReportService>();
            services.AddScoped<ICusPackService, CusPackService>();
            #endregion
            
            return services;
        }
    }
}
