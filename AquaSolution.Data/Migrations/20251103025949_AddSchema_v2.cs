using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSchema_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Admin");

            migrationBuilder.EnsureSchema(
                name: "RequestSuport");

            migrationBuilder.EnsureSchema(
                name: "Clinic");

            migrationBuilder.RenameTable(
                name: "tbl_WarehouseImports",
                newName: "tbl_WarehouseImports",
                newSchema: "Clinic");

            migrationBuilder.RenameTable(
                name: "tbl_WarehouseImportDetails",
                newName: "tbl_WarehouseImportDetails",
                newSchema: "Clinic");

            migrationBuilder.RenameTable(
                name: "tbl_WarehouseExports",
                newName: "tbl_WarehouseExports",
                newSchema: "Clinic");

            migrationBuilder.RenameTable(
                name: "tbl_WarehouseExportDetails",
                newName: "tbl_WarehouseExportDetails",
                newSchema: "Clinic");

            migrationBuilder.RenameTable(
                name: "tbl_Users",
                newName: "tbl_Users",
                newSchema: "Admin");

            migrationBuilder.RenameTable(
                name: "tbl_UserRoles",
                newName: "tbl_UserRoles",
                newSchema: "Admin");

            migrationBuilder.RenameTable(
                name: "tbl_Treatments",
                newName: "tbl_Treatments",
                newSchema: "Clinic");

            migrationBuilder.RenameTable(
                name: "tbl_Roles",
                newName: "tbl_Roles",
                newSchema: "Admin");

            migrationBuilder.RenameTable(
                name: "tbl_RolePermissions",
                newName: "tbl_RolePermissions",
                newSchema: "Admin");

            migrationBuilder.RenameTable(
                name: "tbl_RequestSuports",
                newName: "tbl_RequestSuports",
                newSchema: "RequestSuport");

            migrationBuilder.RenameTable(
                name: "tbl_RequestSuportCategorys",
                newName: "tbl_RequestSuportCategorys",
                newSchema: "RequestSuport");

            migrationBuilder.RenameTable(
                name: "tbl_RequestClinics",
                newName: "tbl_RequestClinics",
                newSchema: "Clinic");

            migrationBuilder.RenameTable(
                name: "tbl_ReportInventoryDetail",
                newName: "tbl_ReportInventoryDetail",
                newSchema: "Clinic");

            migrationBuilder.RenameTable(
                name: "tbl_ReportInventory",
                newName: "tbl_ReportInventory",
                newSchema: "Clinic");

            migrationBuilder.RenameTable(
                name: "tbl_Products",
                newName: "tbl_Products",
                newSchema: "Clinic");

            migrationBuilder.RenameTable(
                name: "tbl_Prescriptions",
                newName: "tbl_Prescriptions",
                newSchema: "Clinic");

            migrationBuilder.RenameTable(
                name: "tbl_PrescriptionDetails",
                newName: "tbl_PrescriptionDetails",
                newSchema: "Clinic");

            migrationBuilder.RenameTable(
                name: "tbl_Positions",
                newName: "tbl_Positions",
                newSchema: "Admin");

            migrationBuilder.RenameTable(
                name: "tbl_Permissions",
                newName: "tbl_Permissions",
                newSchema: "Admin");

            migrationBuilder.RenameTable(
                name: "tbl_Pages",
                newName: "tbl_Pages",
                newSchema: "Admin");

            migrationBuilder.RenameTable(
                name: "tbl_Menus",
                newName: "tbl_Menus",
                newSchema: "Admin");

            migrationBuilder.RenameTable(
                name: "tbl_MedicineSupplyRequests",
                newName: "tbl_MedicineSupplyRequests",
                newSchema: "Clinic");

            migrationBuilder.RenameTable(
                name: "tbl_MedicineSupplyRequestDetails",
                newName: "tbl_MedicineSupplyRequestDetails",
                newSchema: "Clinic");

            migrationBuilder.RenameTable(
                name: "tbl_Medicines",
                newName: "tbl_Medicines",
                newSchema: "Clinic");

            migrationBuilder.RenameTable(
                name: "tbl_MedicalSuply",
                newName: "tbl_MedicalSuply",
                newSchema: "Clinic");

            migrationBuilder.RenameTable(
                name: "tbl_InventoryPeriods",
                newName: "tbl_InventoryPeriods",
                newSchema: "Clinic");

            migrationBuilder.RenameTable(
                name: "tbl_InventoryPeriodDetails",
                newName: "tbl_InventoryPeriodDetails",
                newSchema: "Clinic");

            migrationBuilder.RenameTable(
                name: "tbl_Inventory",
                newName: "tbl_Inventory",
                newSchema: "Clinic");

            migrationBuilder.RenameTable(
                name: "tbl_Groups",
                newName: "tbl_Groups",
                newSchema: "Admin");

            migrationBuilder.RenameTable(
                name: "tbl_Factorys",
                newName: "tbl_Factorys",
                newSchema: "Admin");

            migrationBuilder.RenameTable(
                name: "tbl_Departments",
                newName: "tbl_Departments",
                newSchema: "Admin");

            migrationBuilder.RenameTable(
                name: "tbl_Attachments",
                newName: "tbl_Attachments",
                newSchema: "RequestSuport");

            migrationBuilder.RenameTable(
                name: "tbl_ApprovalFlows",
                newName: "tbl_ApprovalFlows",
                newSchema: "Admin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "tbl_WarehouseImports",
                schema: "Clinic",
                newName: "tbl_WarehouseImports");

            migrationBuilder.RenameTable(
                name: "tbl_WarehouseImportDetails",
                schema: "Clinic",
                newName: "tbl_WarehouseImportDetails");

            migrationBuilder.RenameTable(
                name: "tbl_WarehouseExports",
                schema: "Clinic",
                newName: "tbl_WarehouseExports");

            migrationBuilder.RenameTable(
                name: "tbl_WarehouseExportDetails",
                schema: "Clinic",
                newName: "tbl_WarehouseExportDetails");

            migrationBuilder.RenameTable(
                name: "tbl_Users",
                schema: "Admin",
                newName: "tbl_Users");

            migrationBuilder.RenameTable(
                name: "tbl_UserRoles",
                schema: "Admin",
                newName: "tbl_UserRoles");

            migrationBuilder.RenameTable(
                name: "tbl_Treatments",
                schema: "Clinic",
                newName: "tbl_Treatments");

            migrationBuilder.RenameTable(
                name: "tbl_Roles",
                schema: "Admin",
                newName: "tbl_Roles");

            migrationBuilder.RenameTable(
                name: "tbl_RolePermissions",
                schema: "Admin",
                newName: "tbl_RolePermissions");

            migrationBuilder.RenameTable(
                name: "tbl_RequestSuports",
                schema: "RequestSuport",
                newName: "tbl_RequestSuports");

            migrationBuilder.RenameTable(
                name: "tbl_RequestSuportCategorys",
                schema: "RequestSuport",
                newName: "tbl_RequestSuportCategorys");

            migrationBuilder.RenameTable(
                name: "tbl_RequestClinics",
                schema: "Clinic",
                newName: "tbl_RequestClinics");

            migrationBuilder.RenameTable(
                name: "tbl_ReportInventoryDetail",
                schema: "Clinic",
                newName: "tbl_ReportInventoryDetail");

            migrationBuilder.RenameTable(
                name: "tbl_ReportInventory",
                schema: "Clinic",
                newName: "tbl_ReportInventory");

            migrationBuilder.RenameTable(
                name: "tbl_Products",
                schema: "Clinic",
                newName: "tbl_Products");

            migrationBuilder.RenameTable(
                name: "tbl_Prescriptions",
                schema: "Clinic",
                newName: "tbl_Prescriptions");

            migrationBuilder.RenameTable(
                name: "tbl_PrescriptionDetails",
                schema: "Clinic",
                newName: "tbl_PrescriptionDetails");

            migrationBuilder.RenameTable(
                name: "tbl_Positions",
                schema: "Admin",
                newName: "tbl_Positions");

            migrationBuilder.RenameTable(
                name: "tbl_Permissions",
                schema: "Admin",
                newName: "tbl_Permissions");

            migrationBuilder.RenameTable(
                name: "tbl_Pages",
                schema: "Admin",
                newName: "tbl_Pages");

            migrationBuilder.RenameTable(
                name: "tbl_Menus",
                schema: "Admin",
                newName: "tbl_Menus");

            migrationBuilder.RenameTable(
                name: "tbl_MedicineSupplyRequests",
                schema: "Clinic",
                newName: "tbl_MedicineSupplyRequests");

            migrationBuilder.RenameTable(
                name: "tbl_MedicineSupplyRequestDetails",
                schema: "Clinic",
                newName: "tbl_MedicineSupplyRequestDetails");

            migrationBuilder.RenameTable(
                name: "tbl_Medicines",
                schema: "Clinic",
                newName: "tbl_Medicines");

            migrationBuilder.RenameTable(
                name: "tbl_MedicalSuply",
                schema: "Clinic",
                newName: "tbl_MedicalSuply");

            migrationBuilder.RenameTable(
                name: "tbl_InventoryPeriods",
                schema: "Clinic",
                newName: "tbl_InventoryPeriods");

            migrationBuilder.RenameTable(
                name: "tbl_InventoryPeriodDetails",
                schema: "Clinic",
                newName: "tbl_InventoryPeriodDetails");

            migrationBuilder.RenameTable(
                name: "tbl_Inventory",
                schema: "Clinic",
                newName: "tbl_Inventory");

            migrationBuilder.RenameTable(
                name: "tbl_Groups",
                schema: "Admin",
                newName: "tbl_Groups");

            migrationBuilder.RenameTable(
                name: "tbl_Factorys",
                schema: "Admin",
                newName: "tbl_Factorys");

            migrationBuilder.RenameTable(
                name: "tbl_Departments",
                schema: "Admin",
                newName: "tbl_Departments");

            migrationBuilder.RenameTable(
                name: "tbl_Attachments",
                schema: "RequestSuport",
                newName: "tbl_Attachments");

            migrationBuilder.RenameTable(
                name: "tbl_ApprovalFlows",
                schema: "Admin",
                newName: "tbl_ApprovalFlows");
        }
    }
}
