using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class Update_table_History : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Confirm",
                schema: "ScrapManagement",
                table: "tbl_HistoryScraps");

            migrationBuilder.AddColumn<decimal>(
                name: "ConfirmAmount",
                schema: "ScrapManagement",
                table: "tbl_HistoryScraps",
                type: "decimal(18,4)",
                nullable: true,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ConfirmationStatusType",
                schema: "ScrapManagement",
                table: "tbl_HistoryScraps",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                schema: "ScrapManagement",
                table: "tbl_HistoryScraps",
                type: "decimal(18,4)",
                nullable: true,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Reson",
                schema: "ScrapManagement",
                table: "tbl_HistoryScrapDetails",
                type: "nvarchar(3000)",
                maxLength: 3000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_HistoryScraps_Confirmer",
                schema: "ScrapManagement",
                table: "tbl_HistoryScraps",
                column: "Confirmer");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_HistoryScraps_tbl_Users_Confirmer",
                schema: "ScrapManagement",
                table: "tbl_HistoryScraps",
                column: "Confirmer",
                principalSchema: "Admin",
                principalTable: "tbl_Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_HistoryScraps_tbl_Users_Confirmer",
                schema: "ScrapManagement",
                table: "tbl_HistoryScraps");

            migrationBuilder.DropIndex(
                name: "IX_tbl_HistoryScraps_Confirmer",
                schema: "ScrapManagement",
                table: "tbl_HistoryScraps");

            migrationBuilder.DropColumn(
                name: "ConfirmAmount",
                schema: "ScrapManagement",
                table: "tbl_HistoryScraps");

            migrationBuilder.DropColumn(
                name: "ConfirmationStatusType",
                schema: "ScrapManagement",
                table: "tbl_HistoryScraps");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                schema: "ScrapManagement",
                table: "tbl_HistoryScraps");

            migrationBuilder.DropColumn(
                name: "Reson",
                schema: "ScrapManagement",
                table: "tbl_HistoryScrapDetails");

            migrationBuilder.AddColumn<bool>(
                name: "Confirm",
                schema: "ScrapManagement",
                table: "tbl_HistoryScraps",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
