using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTable_KPIScope_And_ExemptMonth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbl_KPIEmployeeScopes",
                schema: "KPI",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScopeType = table.Column<int>(type: "int", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_KPIEmployeeScopes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_KPIEmployeeExemptMonths",
                schema: "KPI",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScopeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_KPIEmployeeExemptMonths", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_KPIEmployeeExemptMonths_tbl_KPIEmployeeScopes_ScopeId",
                        column: x => x.ScopeId,
                        principalSchema: "KPI",
                        principalTable: "tbl_KPIEmployeeScopes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_KPIEmployeeExemptMonths_ScopeId",
                schema: "KPI",
                table: "tbl_KPIEmployeeExemptMonths",
                column: "ScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_KPIEmployeeExemptMonths_UserId_Year_Month",
                schema: "KPI",
                table: "tbl_KPIEmployeeExemptMonths",
                columns: new[] { "UserId", "Year", "Month" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_KPIEmployeeExemptMonths",
                schema: "KPI");

            migrationBuilder.DropTable(
                name: "tbl_KPIEmployeeScopes",
                schema: "KPI");
        }
    }
}
