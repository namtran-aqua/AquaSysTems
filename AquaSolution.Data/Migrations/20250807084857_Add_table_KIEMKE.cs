using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_table_KIEMKE : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbl_InventoryPeriods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_InventoryPeriods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_InventoryPeriodDetails",
                columns: table => new
                {
                    InventoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InventoryPeriodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActualQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SystemQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RemainingValidity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DateManufacture = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProductType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_InventoryPeriodDetails", x => x.InventoryId);
                    table.ForeignKey(
                        name: "FK_tbl_InventoryPeriodDetails_tbl_InventoryPeriods_InventoryPeriodId",
                        column: x => x.InventoryPeriodId,
                        principalTable: "tbl_InventoryPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_InventoryPeriodDetails_tbl_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "tbl_Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_InventoryPeriodDetails_InventoryPeriodId",
                table: "tbl_InventoryPeriodDetails",
                column: "InventoryPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_InventoryPeriodDetails_ProductId",
                table: "tbl_InventoryPeriodDetails",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_InventoryPeriodDetails");

            migrationBuilder.DropTable(
                name: "tbl_InventoryPeriods");
        }
    }
}
