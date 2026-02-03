using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class update_flowApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_ApprovalFlows_tbl_Positions_PositionId",
                schema: "Admin",
                table: "tbl_ApprovalFlows");

            migrationBuilder.DropIndex(
                name: "IX_tbl_ApprovalFlows_PositionId",
                schema: "Admin",
                table: "tbl_ApprovalFlows");

            migrationBuilder.DropColumn(
                name: "PositionId",
                schema: "Admin",
                table: "tbl_ApprovalFlows");

            migrationBuilder.AddColumn<int>(
                name: "FlowApproval",
                schema: "Admin",
                table: "tbl_Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FlowApproval",
                schema: "Admin",
                table: "tbl_ApprovalFlows",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FlowApproval",
                schema: "Admin",
                table: "tbl_Users");

            migrationBuilder.DropColumn(
                name: "FlowApproval",
                schema: "Admin",
                table: "tbl_ApprovalFlows");

            migrationBuilder.AddColumn<Guid>(
                name: "PositionId",
                schema: "Admin",
                table: "tbl_ApprovalFlows",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ApprovalFlows_PositionId",
                schema: "Admin",
                table: "tbl_ApprovalFlows",
                column: "PositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_ApprovalFlows_tbl_Positions_PositionId",
                schema: "Admin",
                table: "tbl_ApprovalFlows",
                column: "PositionId",
                principalSchema: "Admin",
                principalTable: "tbl_Positions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
