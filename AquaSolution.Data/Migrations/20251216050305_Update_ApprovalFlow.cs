using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class Update_ApprovalFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SystemType",
                schema: "Admin",
                table: "tbl_ApprovalFlows",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "KPI");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SystemType",
                schema: "Admin",
                table: "tbl_ApprovalFlows");
        }
    }
}
