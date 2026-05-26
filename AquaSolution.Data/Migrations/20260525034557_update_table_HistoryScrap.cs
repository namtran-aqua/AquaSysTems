using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class update_table_HistoryScrap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Confirm",
                schema: "ScrapManagement",
                table: "tbl_HistoryScraps",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                schema: "ScrapManagement",
                table: "tbl_HistoryScraps",
                type: "nvarchar(3000)",
                maxLength: 3000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Confirm",
                schema: "ScrapManagement",
                table: "tbl_HistoryScraps");

            migrationBuilder.DropColumn(
                name: "Notes",
                schema: "ScrapManagement",
                table: "tbl_HistoryScraps");
        }
    }
}
