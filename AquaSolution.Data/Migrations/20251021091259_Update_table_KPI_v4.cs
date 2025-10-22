using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class Update_table_KPI_v4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_KPIApprovalTasks_tbl_KPIRequests_SubmitId",
                table: "tbl_KPIApprovalTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_KPITotalScores_tbl_KPIRequests_SubmitId",
                table: "tbl_KPITotalScores");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tbl_KPIRequests",
                table: "tbl_KPIRequests");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tbl_KPIRequests",
                table: "tbl_KPIRequests",
                column: "SubmitId");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_KPIApprovalTasks_tbl_KPIRequests_SubmitId",
                table: "tbl_KPIApprovalTasks",
                column: "SubmitId",
                principalTable: "tbl_KPIRequests",
                principalColumn: "SubmitId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_KPITotalScores_tbl_KPIRequests_SubmitId",
                table: "tbl_KPITotalScores",
                column: "SubmitId",
                principalTable: "tbl_KPIRequests",
                principalColumn: "SubmitId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_KPIApprovalTasks_tbl_KPIRequests_SubmitId",
                table: "tbl_KPIApprovalTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_KPITotalScores_tbl_KPIRequests_SubmitId",
                table: "tbl_KPITotalScores");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tbl_KPIRequests",
                table: "tbl_KPIRequests");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tbl_KPIRequests",
                table: "tbl_KPIRequests",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_KPIApprovalTasks_tbl_KPIRequests_SubmitId",
                table: "tbl_KPIApprovalTasks",
                column: "SubmitId",
                principalTable: "tbl_KPIRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_KPITotalScores_tbl_KPIRequests_SubmitId",
                table: "tbl_KPITotalScores",
                column: "SubmitId",
                principalTable: "tbl_KPIRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
