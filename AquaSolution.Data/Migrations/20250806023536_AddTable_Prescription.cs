using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTable_Prescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "WarehouseImportType",
                table: "tbl_WarehouseImports",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "WarehouseExportType",
                table: "tbl_WarehouseExports",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "tbl_Prescriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PharmacyManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Prescriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Treatments",
                columns: table => new
                {
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Diagnose = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Treatments = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CheckInTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    PharmacyManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TreatmentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Treatments", x => x.RequestId);
                });

            migrationBuilder.CreateTable(
                name: "tbl_PrescriptionDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ManufacturingDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_PrescriptionDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_PrescriptionDetails_tbl_Prescriptions_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalTable: "tbl_Prescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_PrescriptionDetails_PrescriptionId",
                table: "tbl_PrescriptionDetails",
                column: "PrescriptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_PrescriptionDetails");

            migrationBuilder.DropTable(
                name: "tbl_Treatments");

            migrationBuilder.DropTable(
                name: "tbl_Prescriptions");

            migrationBuilder.AlterColumn<int>(
                name: "WarehouseImportType",
                table: "tbl_WarehouseImports",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "WarehouseExportType",
                table: "tbl_WarehouseExports",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
