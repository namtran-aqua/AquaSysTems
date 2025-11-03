using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "KPI");

            migrationBuilder.RenameTable(
                name: "tbl_UserTasks",
                newName: "tbl_UserTasks",
                newSchema: "KPI");

            migrationBuilder.RenameTable(
                name: "tbl_RequestApprovalTasks",
                newName: "tbl_RequestApprovalTasks",
                newSchema: "KPI");

            migrationBuilder.RenameTable(
                name: "tbl_KPITotalScores",
                newName: "tbl_KPITotalScores",
                newSchema: "KPI");

            migrationBuilder.RenameTable(
                name: "tbl_KPITasks",
                newName: "tbl_KPITasks",
                newSchema: "KPI");

            migrationBuilder.RenameTable(
                name: "tbl_KPIRequests",
                newName: "tbl_KPIRequests",
                newSchema: "KPI");

            migrationBuilder.RenameTable(
                name: "tbl_KPIMonthlyTargets",
                newName: "tbl_KPIMonthlyTargets",
                newSchema: "KPI");

            migrationBuilder.RenameTable(
                name: "tbl_KPIMonthlyActuals",
                newName: "tbl_KPIMonthlyActuals",
                newSchema: "KPI");

            migrationBuilder.RenameTable(
                name: "tbl_KPIIndexWeights",
                newName: "tbl_KPIIndexWeights",
                newSchema: "KPI");

            migrationBuilder.RenameTable(
                name: "tbl_KPIDetailScores",
                newName: "tbl_KPIDetailScores",
                newSchema: "KPI");

            //migrationBuilder.RenameTable(
            //    name: "tbl_KPIApprovalTasks",
            //    newName: "tbl_KPIApprovalTasks",
            //    newSchema: "KPI");

            migrationBuilder.RenameTable(
                name: "tbl_KPIActualMasters",
                newName: "tbl_KPIActualMasters",
                newSchema: "KPI");

            migrationBuilder.RenameTable(
                name: "tbl_Formulas",
                newName: "tbl_Formulas",
                newSchema: "KPI");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "tbl_UserTasks",
                schema: "KPI",
                newName: "tbl_UserTasks");

            migrationBuilder.RenameTable(
                name: "tbl_RequestApprovalTasks",
                schema: "KPI",
                newName: "tbl_RequestApprovalTasks");

            migrationBuilder.RenameTable(
                name: "tbl_KPITotalScores",
                schema: "KPI",
                newName: "tbl_KPITotalScores");

            migrationBuilder.RenameTable(
                name: "tbl_KPITasks",
                schema: "KPI",
                newName: "tbl_KPITasks");

            migrationBuilder.RenameTable(
                name: "tbl_KPIRequests",
                schema: "KPI",
                newName: "tbl_KPIRequests");

            migrationBuilder.RenameTable(
                name: "tbl_KPIMonthlyTargets",
                schema: "KPI",
                newName: "tbl_KPIMonthlyTargets");

            migrationBuilder.RenameTable(
                name: "tbl_KPIMonthlyActuals",
                schema: "KPI",
                newName: "tbl_KPIMonthlyActuals");

            migrationBuilder.RenameTable(
                name: "tbl_KPIIndexWeights",
                schema: "KPI",
                newName: "tbl_KPIIndexWeights");

            migrationBuilder.RenameTable(
                name: "tbl_KPIDetailScores",
                schema: "KPI",
                newName: "tbl_KPIDetailScores");

            migrationBuilder.RenameTable(
                name: "tbl_KPIApprovalTasks",
                schema: "KPI",
                newName: "tbl_KPIApprovalTasks");

            migrationBuilder.RenameTable(
                name: "tbl_KPIActualMasters",
                schema: "KPI",
                newName: "tbl_KPIActualMasters");

            migrationBuilder.RenameTable(
                name: "tbl_Formulas",
                schema: "KPI",
                newName: "tbl_Formulas");
        }
    }
}
