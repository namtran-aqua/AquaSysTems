using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTable_MedicinSupplyRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RejectDate",
                table: "tbl_MedicineSupplyRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RejectId",
                table: "tbl_MedicineSupplyRequests",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectDate",
                table: "tbl_MedicineSupplyRequests");

            migrationBuilder.DropColumn(
                name: "RejectId",
                table: "tbl_MedicineSupplyRequests");
        }
    }
}
