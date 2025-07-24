using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class Update_forenkey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
      

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "tbl_Users",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(555)",
                oldMaxLength: 555);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "tbl_Groups",
                type: "nvarchar(max)",
                maxLength: 5000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 5000);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Users_GroupId",
                table: "tbl_Users",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Users_tbl_Groups_GroupId",
                table: "tbl_Users",
                column: "GroupId",
                principalTable: "tbl_Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Users_tbl_Groups_GroupId",
                table: "tbl_Users");

            migrationBuilder.DropIndex(
                name: "IX_tbl_Users_GroupId",
                table: "tbl_Users");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "tbl_Users",
                type: "nvarchar(555)",
                maxLength: 555,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "tbl_Groups",
                type: "nvarchar(max)",
                maxLength: 5000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 5000,
                oldNullable: true);
           
        }
    }
}
