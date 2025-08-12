using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class add_table_export : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbl_WarehouseExports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WarehouseExportType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_WarehouseExports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_WarehouseExportDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WarehouseExportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateManufacture = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ProductType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_WarehouseExportDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_WarehouseExportDetails_tbl_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "tbl_Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_WarehouseExportDetails_tbl_WarehouseExports_WarehouseExportId",
                        column: x => x.WarehouseExportId,
                        principalTable: "tbl_WarehouseExports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_WarehouseExportDetails_ProductId",
                table: "tbl_WarehouseExportDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_WarehouseExportDetails_WarehouseExportId",
                table: "tbl_WarehouseExportDetails",
                column: "WarehouseExportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_WarehouseExportDetails");

            migrationBuilder.DropTable(
                name: "tbl_WarehouseExports");
        }
    }
}
