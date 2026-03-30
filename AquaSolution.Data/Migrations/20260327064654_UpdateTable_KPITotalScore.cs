using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTable_KPITotalScore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalActualScore",
                schema: "KPI",
                table: "tbl_KPITotalScores",
                type: "decimal(18,4)",
                nullable: true,
                defaultValue: 0m);

            migrationBuilder.Sql(@"
                UPDATE KPI.tbl_KPITotalScores
                SET TotalActualScore = TotaleScore
                WHERE TotalActualScore IS NULL OR TotalActualScore = 0
            ");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalActualScore",
                schema: "KPI",
                table: "tbl_KPITotalScores");
        }
    }
}
