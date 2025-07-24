using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Leval2",
                table: "tbl_Users");

            migrationBuilder.DropColumn(
                name: "Leval3",
                table: "tbl_Users");

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "tbl_Pages",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "tbl_Pages");

            migrationBuilder.AddColumn<Guid>(
                name: "Leval2",
                table: "tbl_Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Leval3",
                table: "tbl_Users",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}
