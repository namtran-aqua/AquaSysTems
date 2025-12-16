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
            // Drop FK dùng SubmitId (FK vẫn giữ, chỉ đổi PK target)
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_KPIApprovalTasks_tbl_KPIRequests_SubmitId",
                table: "tbl_KPIApprovalTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_KPITotalScores_tbl_KPIRequests_SubmitId",
                table: "tbl_KPITotalScores");

            // Drop PK cũ
            migrationBuilder.DropPrimaryKey(
                name: "PK_tbl_KPIRequests",
                table: "tbl_KPIRequests");

            // ✅ ADD PK ĐÚNG: Id
            migrationBuilder.AddPrimaryKey(
                name: "PK_tbl_KPIRequests",
                table: "tbl_KPIRequests",
                column: "Id");

            // ✅ FK trỏ về KPIRequests.Id
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop FK
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_KPIApprovalTasks_tbl_KPIRequests_SubmitId",
                table: "tbl_KPIApprovalTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_KPITotalScores_tbl_KPIRequests_SubmitId",
                table: "tbl_KPITotalScores");

            // Drop PK Id
            migrationBuilder.DropPrimaryKey(
                name: "PK_tbl_KPIRequests",
                table: "tbl_KPIRequests");

            // ⚠️ Rollback về SubmitId (chỉ để Down đúng chuẩn migration)
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
    }
}
