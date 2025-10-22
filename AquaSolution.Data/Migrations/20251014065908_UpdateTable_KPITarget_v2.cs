using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTable_KPIMonthlyTarget_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
  
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Quarter",
                table: "tbl_KPIMonthlyTargets",
                newName: "Quater");

            migrationBuilder.RenameColumn(
                name: "HalfYear",
                table: "tbl_KPIMonthlyTargets",
                newName: "HaftYear");
        }
    }
}
