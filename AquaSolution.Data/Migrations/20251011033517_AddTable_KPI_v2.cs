using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTable_KPI_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbl_KPIRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovalBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RejectDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_KPIRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_KPIRequests_tbl_Users_ApprovalBy",
                        column: x => x.ApprovalBy,
                        principalTable: "tbl_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_KPIRequests_tbl_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "tbl_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_KPIRequests_tbl_Users_RejectBy",
                        column: x => x.RejectBy,
                        principalTable: "tbl_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_KPIMonthlyTargets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    TargetValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_KPIMonthlyTargets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_KPIMonthlyTargets_tbl_UserTasks_UserTaskId",
                        column: x => x.UserTaskId,
                        principalTable: "tbl_UserTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_KPIMonthlyTargets_tbl_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "tbl_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_KPIMonthlyTargets_tbl_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "tbl_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_KPIApprovalTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KPIRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequesterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RejectDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_KPIApprovalTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_KPIApprovalTasks_tbl_KPIRequests_KPIRequestId",
                        column: x => x.KPIRequestId,
                        principalTable: "tbl_KPIRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_KPIApprovalTasks_tbl_Users_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalTable: "tbl_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_KPIApprovalTasks_tbl_Users_RejectBy",
                        column: x => x.RejectBy,
                        principalTable: "tbl_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_KPIApprovalTasks_tbl_Users_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "tbl_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_KPITotalScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KPIRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KPIScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KeyTaskScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OMGScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: true),
                    Quarter = table.Column<int>(type: "int", nullable: true),
                    HalfYear = table.Column<int>(type: "int", nullable: true),
                    Year = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_KPITotalScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_KPITotalScores_tbl_KPIRequests_KPIRequestId",
                        column: x => x.KPIRequestId,
                        principalTable: "tbl_KPIRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_KPIMonthlyActuals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KPIMonthlyTargetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KPITotalScoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    TargetValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_KPIMonthlyActuals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_KPIMonthlyActuals_tbl_KPIMonthlyTargets_KPIMonthlyTargetId",
                        column: x => x.KPIMonthlyTargetId,
                        principalTable: "tbl_KPIMonthlyTargets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_KPIMonthlyActuals_tbl_KPITotalScores_KPITotalScoreId",
                        column: x => x.KPITotalScoreId,
                        principalTable: "tbl_KPITotalScores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_KPIMonthlyActuals_tbl_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "tbl_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_KPIMonthlyActuals_KPIMonthlyTargetId",
                table: "tbl_KPIMonthlyActuals",
                column: "KPIMonthlyTargetId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_KPIMonthlyActuals_KPITotalScoreId",
                table: "tbl_KPIMonthlyActuals",
                column: "KPITotalScoreId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_KPIMonthlyActuals_UpdatedBy",
                table: "tbl_KPIMonthlyActuals",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_KPIApprovalTasks_ApprovedBy",
                table: "tbl_KPIApprovalTasks",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_KPIApprovalTasks_KPIRequestId",
                table: "tbl_KPIApprovalTasks",
                column: "KPIRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_KPIApprovalTasks_RejectBy",
                table: "tbl_KPIApprovalTasks",
                column: "RejectBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_KPIApprovalTasks_RequesterId",
                table: "tbl_KPIApprovalTasks",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_KPIRequests_ApprovalBy",
                table: "tbl_KPIRequests",
                column: "ApprovalBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_KPIRequests_CreatedBy",
                table: "tbl_KPIRequests",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_KPIRequests_RejectBy",
                table: "tbl_KPIRequests",
                column: "RejectBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_KPIMonthlyTargets_UpdatedBy",
                table: "tbl_KPIMonthlyTargets",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_KPIMonthlyTargets_UserId",
                table: "tbl_KPIMonthlyTargets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_KPIMonthlyTargets_UserTaskId",
                table: "tbl_KPIMonthlyTargets",
                column: "UserTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_KPITotalScores_KPIRequestId",
                table: "tbl_KPITotalScores",
                column: "KPIRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_KPIMonthlyActuals");

            migrationBuilder.DropTable(
                name: "tbl_KPIApprovalTasks");

            migrationBuilder.DropTable(
                name: "tbl_KPIMonthlyTargets");

            migrationBuilder.DropTable(
                name: "tbl_KPITotalScores");

            migrationBuilder.DropTable(
                name: "tbl_KPIRequests");
        }
    }
}
