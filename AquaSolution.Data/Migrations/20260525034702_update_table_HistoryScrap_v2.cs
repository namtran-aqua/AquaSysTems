using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class update_table_HistoryScrap_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmDate",
                schema: "ScrapManagement",
                table: "tbl_HistoryScraps",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Confirmer",
                schema: "ScrapManagement",
                table: "tbl_HistoryScraps",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfirmDate",
                schema: "ScrapManagement",
                table: "tbl_HistoryScraps");

            migrationBuilder.DropColumn(
                name: "Confirmer",
                schema: "ScrapManagement",
                table: "tbl_HistoryScraps");
        }
    }
}
