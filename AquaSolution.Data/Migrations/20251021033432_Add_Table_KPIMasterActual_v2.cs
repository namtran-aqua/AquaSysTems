using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_Table_KPIMasterActual_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_KPITasks_tbl_QuaterCalculateds_QuaterCalculatedId",
                table: "tbl_KPITasks");

            migrationBuilder.DropTable(
                name: "tbl_QuaterCalculateds");

            migrationBuilder.DropIndex(
                name: "IX_tbl_KPITasks_QuaterCalculatedId",
                table: "tbl_KPITasks");

            migrationBuilder.DropColumn(
                name: "QuaterCalculatedId",
                table: "tbl_KPITasks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "QuaterCalculatedId",
                table: "tbl_KPITasks",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "tbl_QuaterCalculateds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Calculated = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    KPIQuarterCalculateType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_QuaterCalculateds", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_KPITasks_QuaterCalculatedId",
                table: "tbl_KPITasks",
                column: "QuaterCalculatedId");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_KPITasks_tbl_QuaterCalculateds_QuaterCalculatedId",
                table: "tbl_KPITasks",
                column: "QuaterCalculatedId",
                principalTable: "tbl_QuaterCalculateds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
