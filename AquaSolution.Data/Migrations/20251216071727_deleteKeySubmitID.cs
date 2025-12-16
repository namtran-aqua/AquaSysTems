using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class deleteKeySubmitID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_KPIApprovalTasks_tbl_KPIRequests_SubmitId",
                schema: "KPI",
                table: "tbl_KPIApprovalTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_KPITotalScores_tbl_KPIRequests_SubmitId",
                schema: "KPI",
                table: "tbl_KPITotalScores");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_RequestApprovalTasks_tbl_KPIRequests_SubmitId",
                schema: "KPI",
                table: "tbl_RequestApprovalTasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tbl_KPIRequests",
                schema: "KPI",
                table: "tbl_KPIRequests");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                schema: "KPI",
                table: "tbl_KPIRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_tbl_KPIRequests",
                schema: "KPI",
                table: "tbl_KPIRequests",
                column: "Id");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_tbl_KPIApprovalTasks_tbl_KPIRequests_SubmitId",
            //    schema: "KPI",
            //    table: "tbl_KPIApprovalTasks",
            //    column: "SubmitId",
            //    principalSchema: "KPI",
            //    principalTable: "tbl_KPIRequests",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Restrict);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_tbl_KPITotalScores_tbl_KPIRequests_SubmitId",
            //    schema: "KPI",
            //    table: "tbl_KPITotalScores",
            //    column: "SubmitId",
            //    principalSchema: "KPI",
            //    principalTable: "tbl_KPIRequests",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Restrict);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_tbl_RequestApprovalTasks_tbl_KPIRequests_SubmitId",
            //    schema: "KPI",
            //    table: "tbl_RequestApprovalTasks",
            //    column: "SubmitId",
            //    principalSchema: "KPI",
            //    principalTable: "tbl_KPIRequests",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_KPIApprovalTasks_tbl_KPIRequests_SubmitId",
                schema: "KPI",
                table: "tbl_KPIApprovalTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_KPITotalScores_tbl_KPIRequests_SubmitId",
                schema: "KPI",
                table: "tbl_KPITotalScores");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_RequestApprovalTasks_tbl_KPIRequests_SubmitId",
                schema: "KPI",
                table: "tbl_RequestApprovalTasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tbl_KPIRequests",
                schema: "KPI",
                table: "tbl_KPIRequests");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "KPI",
                table: "tbl_KPIRequests");

            //migrationBuilder.AddPrimaryKey(
            //    name: "PK_tbl_KPIRequests",
            //    schema: "KPI",
            //    table: "tbl_KPIRequests",
            //    column: "SubmitId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_tbl_KPIApprovalTasks_tbl_KPIRequests_SubmitId",
            //    schema: "KPI",
            //    table: "tbl_KPIApprovalTasks",
            //    column: "SubmitId",
            //    principalSchema: "KPI",
            //    principalTable: "tbl_KPIRequests",
            //    principalColumn: "SubmitId",
            //    onDelete: ReferentialAction.Restrict);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_tbl_KPITotalScores_tbl_KPIRequests_SubmitId",
            //    schema: "KPI",
            //    table: "tbl_KPITotalScores",
            //    column: "SubmitId",
            //    principalSchema: "KPI",
            //    principalTable: "tbl_KPIRequests",
            //    principalColumn: "SubmitId",
            //    onDelete: ReferentialAction.Restrict);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_tbl_RequestApprovalTasks_tbl_KPIRequests_SubmitId",
            //    schema: "KPI",
            //    table: "tbl_RequestApprovalTasks",
            //    column: "SubmitId",
            //    principalSchema: "KPI",
            //    principalTable: "tbl_KPIRequests",
            //    principalColumn: "SubmitId",
            //    onDelete: ReferentialAction.Restrict);
        }
    }
}
