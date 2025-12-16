using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class Update_Table_Flow_v3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbl_RequestApprovalTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequesterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatusType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RejectDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_RequestApprovalTasks", x => x.Id);

                    // ✅ FK ĐÚNG: SubmitId -> tbl_KPIRequests.Id
                    table.ForeignKey(
                        name: "FK_tbl_RequestApprovalTasks_tbl_KPIRequests_SubmitId",
                        column: x => x.SubmitId,
                        principalTable: "tbl_KPIRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_tbl_RequestApprovalTasks_tbl_Users_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalTable: "tbl_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_tbl_RequestApprovalTasks_tbl_Users_RejectBy",
                        column: x => x.RejectBy,
                        principalTable: "tbl_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_tbl_RequestApprovalTasks_tbl_Users_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "tbl_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_RequestApprovalTasks_ApprovedBy",
                table: "tbl_RequestApprovalTasks",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_RequestApprovalTasks_RejectBy",
                table: "tbl_RequestApprovalTasks",
                column: "RejectBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_RequestApprovalTasks_RequesterId",
                table: "tbl_RequestApprovalTasks",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_RequestApprovalTasks_SubmitId",
                table: "tbl_RequestApprovalTasks",
                column: "SubmitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_RequestApprovalTasks");
        }
    }
}
