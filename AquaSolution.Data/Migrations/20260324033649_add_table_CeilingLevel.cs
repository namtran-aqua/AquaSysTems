using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class add_table_CeilingLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbl_CeilingLevels",
                schema: "KPI",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FactoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CeilingLevelValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_CeilingLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_CeilingLevels_tbl_Factorys_FactoryId",
                        column: x => x.FactoryId,
                        principalSchema: "Admin",
                        principalTable: "tbl_Factorys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_CeilingLevels_tbl_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "Admin",
                        principalTable: "tbl_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_CeilingLevels_CreatedBy",
                schema: "KPI",
                table: "tbl_CeilingLevels",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_CeilingLevels_FactoryId",
                schema: "KPI",
                table: "tbl_CeilingLevels",
                column: "FactoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_CeilingLevels",
                schema: "KPI");
        }
    }
}
