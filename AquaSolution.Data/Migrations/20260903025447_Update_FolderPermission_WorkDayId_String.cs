using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class Update_FolderPermission_WorkDayId_String : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Imgs");

            migrationBuilder.CreateTable(
                name: "tbl_FolderPermissions",
                schema: "Imgs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkDayId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FolderId = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    FolderName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_FolderPermissions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_FolderPermissions",
                schema: "Imgs");
        }
    }
}
