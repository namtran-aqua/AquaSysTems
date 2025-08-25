using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class addTable_v1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "PharmacyManagerId",
                table: "tbl_Prescriptions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<decimal>(
                name: "SystemQuantity",
                table: "tbl_InventoryPeriodDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "tbl_MedicineSupplyRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FactoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PharmacyManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MedicineDispensingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_MedicineSupplyRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_MedicineSupplyRequests_tbl_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "tbl_Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_MedicineSupplyRequests_tbl_Factorys_FactoryId",
                        column: x => x.FactoryId,
                        principalTable: "tbl_Factorys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_MedicineSupplyRequests_tbl_Users_ApprovalId",
                        column: x => x.ApprovalId,
                        principalTable: "tbl_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_MedicineSupplyRequests_tbl_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "tbl_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_MedicineSupplyRequests_tbl_Users_PharmacyManagerId",
                        column: x => x.PharmacyManagerId,
                        principalTable: "tbl_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_MedicineSupplyRequests_tbl_Users_UserRequestId",
                        column: x => x.UserRequestId,
                        principalTable: "tbl_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_MedicineSupplyRequestDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicineSupplyRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateManufactured = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequestedQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    QuantityIssued = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_MedicineSupplyRequestDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_MedicineSupplyRequestDetails_tbl_MedicineSupplyRequests_MedicineSupplyRequestId",
                        column: x => x.MedicineSupplyRequestId,
                        principalTable: "tbl_MedicineSupplyRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_MedicineSupplyRequestDetails_tbl_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "tbl_Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_MedicineSupplyRequestDetails_MedicineSupplyRequestId",
                table: "tbl_MedicineSupplyRequestDetails",
                column: "MedicineSupplyRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_MedicineSupplyRequestDetails_ProductId",
                table: "tbl_MedicineSupplyRequestDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_MedicineSupplyRequests_ApprovalId",
                table: "tbl_MedicineSupplyRequests",
                column: "ApprovalId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_MedicineSupplyRequests_CreatedById",
                table: "tbl_MedicineSupplyRequests",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_MedicineSupplyRequests_DepartmentId",
                table: "tbl_MedicineSupplyRequests",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_MedicineSupplyRequests_FactoryId",
                table: "tbl_MedicineSupplyRequests",
                column: "FactoryId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_MedicineSupplyRequests_PharmacyManagerId",
                table: "tbl_MedicineSupplyRequests",
                column: "PharmacyManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_MedicineSupplyRequests_UserRequestId",
                table: "tbl_MedicineSupplyRequests",
                column: "UserRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_MedicineSupplyRequestDetails");

            migrationBuilder.DropTable(
                name: "tbl_MedicineSupplyRequests");

            migrationBuilder.AlterColumn<Guid>(
                name: "PharmacyManagerId",
                table: "tbl_Prescriptions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SystemQuantity",
                table: "tbl_InventoryPeriodDetails",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}
