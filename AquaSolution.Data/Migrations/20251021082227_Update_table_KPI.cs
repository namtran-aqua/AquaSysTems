using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class Update_table_KPI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
      name: "FK_tbl_KPIApprovalTasks_tbl_KPIRequests_KPIRequestId",
      table: "tbl_KPIApprovalTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_KPITotalScores_tbl_KPIRequests_KPIRequestId",
                table: "tbl_KPITotalScores");

            // Đổi tên cột KPIRequestId -> SubmitId (không tạo cột mới!)
            migrationBuilder.RenameColumn(
                name: "KPIRequestId",
                table: "tbl_KPITotalScores",
                newName: "SubmitId");

            migrationBuilder.RenameIndex(
                name: "IX_tbl_KPITotalScores_KPIRequestId",
                table: "tbl_KPITotalScores",
                newName: "IX_tbl_KPITotalScores_SubmitId");

            migrationBuilder.RenameColumn(
                name: "KPIRequestId",
                table: "tbl_KPIApprovalTasks",
                newName: "SubmitId");

            migrationBuilder.RenameIndex(
                name: "IX_tbl_KPIApprovalTasks_KPIRequestId",
                table: "tbl_KPIApprovalTasks",
                newName: "IX_tbl_KPIApprovalTasks_SubmitId");

            // Thêm cột mới "Title" nếu cần
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "tbl_KPITotalScores",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            // Foreign Key mới dùng SubmitId
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
            // Drop updated foreign keys
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_KPIApprovalTasks_tbl_KPIRequests_SubmitId",
                table: "tbl_KPIApprovalTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_KPIMonthlyActuals_tbl_KPIMonthlyTargets_KPITargetId",
                table: "tbl_KPIMonthlyActuals");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_KPITotalScores_tbl_KPIRequests_SubmitId",
                table: "tbl_KPITotalScores");

            // Drop added columns
            migrationBuilder.DropColumn(
                name: "Title",
                table: "tbl_KPITotalScores");

            migrationBuilder.DropColumn(
                name: "SubmitId",
                table: "tbl_KPIRequests");

            // Rename SubmitId to KPIRequestId
            migrationBuilder.RenameColumn(
                name: "SubmitId",
                table: "tbl_KPITotalScores",
                newName: "KPIRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_tbl_KPITotalScores_SubmitId",
                table: "tbl_KPITotalScores",
                newName: "IX_tbl_KPITotalScores_KPIRequestId");

            // Rename KPITargetId back to KPIMonthlyTargetId
            migrationBuilder.RenameColumn(
                name: "KPITargetId",
                table: "tbl_KPIMonthlyActuals",
                newName: "KPIMonthlyTargetId");

            migrationBuilder.RenameIndex(
                name: "IX_tbl_KPIMonthlyActuals_KPITargetId",
                table: "tbl_KPIMonthlyActuals",
                newName: "IX_tbl_KPIMonthlyActuals_KPIMonthlyTargetId");

            // Rename SubmitId back to KPIRequestId
            migrationBuilder.RenameColumn(
                name: "SubmitId",
                table: "tbl_KPIApprovalTasks",
                newName: "KPIRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_tbl_KPIApprovalTasks_SubmitId",
                table: "tbl_KPIApprovalTasks",
                newName: "IX_tbl_KPIApprovalTasks_KPIRequestId");

            // Re-add original foreign keys
            migrationBuilder.AddForeignKey(
                name: "FK_tbl_KPIApprovalTasks_tbl_KPIRequests_KPIRequestId",
                table: "tbl_KPIApprovalTasks",
                column: "KPIRequestId",
                principalTable: "tbl_KPIRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_KPIMonthlyActuals_tbl_KPIMonthlyTargets_KPIMonthlyTargetId",
                table: "tbl_KPIMonthlyActuals",
                column: "KPIMonthlyTargetId",
                principalTable: "tbl_KPIMonthlyTargets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_KPITotalScores_tbl_KPIRequests_KPIRequestId",
                table: "tbl_KPITotalScores",
                column: "KPIRequestId",
                principalTable: "tbl_KPIRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
