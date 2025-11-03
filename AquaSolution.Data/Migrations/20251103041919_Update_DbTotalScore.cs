using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class Update_DbTotalScore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "kPITotalScoreType",
                schema: "KPI",
                table: "tbl_KPITotalScores",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "Staff");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "kPITotalScoreType",
                schema: "KPI",
                table: "tbl_KPITotalScores");
        }
    }
}
