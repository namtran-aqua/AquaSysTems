using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Data.MappingConfigurations;
using Microsoft.EntityFrameworkCore;

namespace AquaSolution.Data.Connection
{
    public class AquaDbContext : DbContext
    {
        public AquaDbContext(DbContextOptions<AquaDbContext> options) : base(options)
        {
        }
        // public DbSet<Button> Buttons { get; set; }
        #region Admin (Role, Permission)
        public DbSet<Menu> tbl_Menus { get; set; }
        public DbSet<Page> tbl_Pages { get; set; }
        public DbSet<Permission> tbl_Permissions { get; set; }
        public DbSet<Role> tbl_Roles { get; set; }
        public DbSet<RolePermission> tbl_RolePermissions { get; set; }
        public DbSet<User> tbl_Users { get; set; }
        public DbSet<UserRole> tbl_UserRoles { get; set; }
        public DbSet<Groups> tbl_Groups { get; set; }
        public DbSet<Department> tbl_Departments { get; set; }
        public DbSet<Factory> tbl_Factorys { get; set; }
        public DbSet<Position> tbl_Positions { get; set; }
        public DbSet<ApprovalFlow> tbl_ApprovalFlow { get; set; }
        #endregion
        #region Clinic
        public DbSet<Product> tbl_Product { get; set; }
        public DbSet<Inventories> tbl_Inventory { get; set; }
        public DbSet<MedicalSuplies> tbl_MedicalSuply { get; set; }
        public DbSet<Medicine> tbl_Medicine { get; set; }
        public DbSet<RequestClinic> tbl_RequestClinic { get; set; }
        public DbSet<WarehouseImport> tbl_WarehouseImport { get; set; }
        public DbSet<WarehouseImportDetail> tbl_WarehouseImportDetail { get; set; }
        public DbSet<WarehouseExport> tbl_WarehouseExport { get; set; }
        public DbSet<WarehouseExportDetail> tbl_WarehouseExportDetail { get; set; }
        public DbSet<SysTemHistory> tbl_SysTemHistory { get; set; }
        public DbSet<Treatment> tbl_Treatment { get; set; }
        public DbSet<Prescription> tbl_Prescription { get; set; }
        public DbSet<PrescriptionDetail> tbl_PrescriptionDetail { get; set; }
        public DbSet<InventoryPeriod> tbl_InventoryPeriod { get; set; }
        public DbSet<InventoryPeriodDetail> tbl_InventoryPeriodDetail { get; set; }
        public DbSet<MedicineSupplyRequest> tbl_MedicineSupplyRequest { get; set; }
        public DbSet<MedicineSupplyRequestDetail> tbl_MedicineSupplyRequestDetail { get; set; }
        public DbSet<ReportInventoryDetail> tbl_ReportInventoryDetail { get; set; }
        public DbSet<ReportInventory> tbl_ReportInventory { get; set; }


        #endregion
        #region IT Suport
        public DbSet<RequestSuportCategory> tbl_RequestSuportCategory { get; set; }
        public DbSet<RequestSuport> tbl_RequestSuport { get; set; }
        public DbSet<Attachment> tbl_Attachment { get; set; }
        #endregion
        #region KPI
        public DbSet<QuaterCalculated> tbl_QuaterCalculateds { get; set; }
        public DbSet<Formula> tbl_Formulas { get; set; }
        public DbSet<KPITask> tbl_KPITasks { get; set; }
        public DbSet<UserTask> tbl_UserTasks { get; set; }

        #endregion
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new MenuConfiguration());
            modelBuilder.ApplyConfiguration(new PageConfiguration());
            modelBuilder.ApplyConfiguration(new PermissionConfiguration());
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
            modelBuilder.ApplyConfiguration(new GroupsConfiguration());
            modelBuilder.ApplyConfiguration(new DepartmentConfiguration());   
            modelBuilder.ApplyConfiguration(new FactoryConfiguration());
            modelBuilder.ApplyConfiguration(new PositionConfiguration());
            modelBuilder.ApplyConfiguration(new ApprovalFlowConfiguration());
            #region Clinic
            modelBuilder.ApplyConfiguration(new RequestClinicConfiguration());
            modelBuilder.ApplyConfiguration(new ProductConfiguration());
            modelBuilder.ApplyConfiguration(new InventoryConfiguration());
            modelBuilder.ApplyConfiguration(new MedicalSupliesConfiguration());
            modelBuilder.ApplyConfiguration(new MedicineConfiguration());
            modelBuilder.ApplyConfiguration(new WarehouseImportDetailConfiguration());
            modelBuilder.ApplyConfiguration(new WarehouseImportConfiguration());
            modelBuilder.ApplyConfiguration(new WarehouseExportDetailConfiguration());
            modelBuilder.ApplyConfiguration(new WarehouseExportConfiguration());
            modelBuilder.ApplyConfiguration(new SysTemHistoryConfiguration());
            modelBuilder.ApplyConfiguration(new TreatmentConfiguration());
            modelBuilder.ApplyConfiguration(new PrescriptionConfiguration());
            modelBuilder.ApplyConfiguration(new PrescriptionDetailConfiguration());
            modelBuilder.ApplyConfiguration(new InventoryPeriodDetailConfiguration());
            modelBuilder.ApplyConfiguration(new InventoryPeriodConfiguration());
            modelBuilder.ApplyConfiguration(new MedicineSupplyRequestConfiguration());
            modelBuilder.ApplyConfiguration(new MedicineSupplyRequestDetailConfiguration());
            modelBuilder.ApplyConfiguration(new ReportInventoryConfiguration());
            modelBuilder.ApplyConfiguration(new ReportInventoryDetailConfiguration());

            #endregion
            #region IT Suport

            modelBuilder.ApplyConfiguration(new AttachmentConfiguration());
            modelBuilder.ApplyConfiguration(new RequestSuportConfiguration());
            modelBuilder.ApplyConfiguration(new RequestSuportCategoryConfiguration());

            #endregion
            #region KPI
            modelBuilder.ApplyConfiguration(new KPITaskConfiguration());
            modelBuilder.ApplyConfiguration(new FormulaConfiguration());
            modelBuilder.ApplyConfiguration(new QuaterCalculatedConfiguration());
            modelBuilder.ApplyConfiguration(new UserTaskConfiguration());

            #endregion

        }
    }
}
