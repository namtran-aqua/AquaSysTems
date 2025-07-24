using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Created",
                table: "tbl_Users",
                newName: "CreatedTime");

            migrationBuilder.AddColumn<string>(
                name: "Avatar",
                table: "tbl_Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "tbl_Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdateBy",
                table: "tbl_Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedTime",
                table: "tbl_Users",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Avatar",
                table: "tbl_Users");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "tbl_Users");

            migrationBuilder.DropColumn(
                name: "UpdateBy",
                table: "tbl_Users");

            migrationBuilder.DropColumn(
                name: "UpdatedTime",
                table: "tbl_Users");

            migrationBuilder.RenameColumn(
                name: "CreatedTime",
                table: "tbl_Users",
                newName: "Created");
        }
    }
}
