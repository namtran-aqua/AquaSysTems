using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class Update_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "tbl_WarehouseImports");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "tbl_WarehouseExports");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "tbl_RequestSuports",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_tbl_RequestSuports_CreatedById",
                table: "tbl_RequestSuports",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_RequestSuports_tbl_Users_CreatedById",
                table: "tbl_RequestSuports",
                column: "CreatedById",
                principalTable: "tbl_Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_RequestSuports_tbl_Users_CreatedById",
                table: "tbl_RequestSuports");

            migrationBuilder.DropIndex(
                name: "IX_tbl_RequestSuports_CreatedById",
                table: "tbl_RequestSuports");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "tbl_RequestSuports");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "tbl_WarehouseImports",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "tbl_WarehouseExports",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
