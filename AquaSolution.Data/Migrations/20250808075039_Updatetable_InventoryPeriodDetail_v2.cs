using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class Updatetable_InventoryPeriodDetail_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_tbl_InventoryPeriodDetails",
                table: "tbl_InventoryPeriodDetails");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tbl_InventoryPeriodDetails",
                table: "tbl_InventoryPeriodDetails",
                columns: new[] { "InventoryId", "InventoryPeriodId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_tbl_InventoryPeriodDetails",
                table: "tbl_InventoryPeriodDetails");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tbl_InventoryPeriodDetails",
                table: "tbl_InventoryPeriodDetails",
                column: "InventoryId");
        }
    }
}
