using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_Table_KPIMasterActual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HalfYear",
                table: "tbl_KPIMonthlyTargets");

            migrationBuilder.DropColumn(
                name: "Quarter",
                table: "tbl_KPIMonthlyTargets");

            migrationBuilder.DropColumn(
                name: "HaftYear",
                table: "tbl_KPIMonthlyActuals");
            migrationBuilder.AddColumn<decimal>(
                name: "TotaleScore",
                table: "tbl_KPITotalScores",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tbl_KPIActualMasters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quarter = table.Column<int>(type: "int", nullable: true),
                    HaflYear = table.Column<int>(type: "int", nullable: true),
                    Year = table.Column<int>(type: "int", nullable: false),
                    KPIScore = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    KeyTaskScore = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    OMGScore = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotaleScore = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_KPIActualMasters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_KPIActualMasters_tbl_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "tbl_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_KPIActualMasters_UserId",
                table: "tbl_KPIActualMasters",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_KPIActualMasters");

            migrationBuilder.DropColumn(
                name: "TotaleScore",
                table: "tbl_KPITotalScores");

            migrationBuilder.AddColumn<int>(
                name: "HalfYear",
                table: "tbl_KPIMonthlyTargets",
                type: "int",
                nullable: true);



            migrationBuilder.AddColumn<int>(
                name: "HaftYear",
                table: "tbl_KPIMonthlyActuals",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Quater",
                table: "tbl_KPIMonthlyActuals",
                type: "int",
                nullable: true);
        }
    }
}
