using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTable_Prescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RequestId",
                table: "tbl_Prescriptions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Prescriptions_RequestId",
                table: "tbl_Prescriptions",
                column: "RequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Prescriptions_tbl_RequestClinics_RequestId",
                table: "tbl_Prescriptions",
                column: "RequestId",
                principalTable: "tbl_RequestClinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Prescriptions_tbl_RequestClinics_RequestId",
                table: "tbl_Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_tbl_Prescriptions_RequestId",
                table: "tbl_Prescriptions");

            migrationBuilder.DropColumn(
                name: "RequestId",
                table: "tbl_Prescriptions");
        }
    }
}
