using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTable_KPI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbl_Formulas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FormulaName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KPIFormulaType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Formulas", x => x.Id);
                });

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

            migrationBuilder.CreateTable(
                name: "tbl_KPITasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    KPICategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TaskDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CalculatedMdethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataSource = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KPIIndexType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    QuaterCalculatedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FormulaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Max = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Bottom = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FactoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_KPITasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_KPITasks_tbl_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "tbl_Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_KPITasks_tbl_Factorys_FactoryId",
                        column: x => x.FactoryId,
                        principalTable: "tbl_Factorys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_KPITasks_tbl_Formulas_FormulaId",
                        column: x => x.FormulaId,
                        principalTable: "tbl_Formulas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_KPITasks_tbl_QuaterCalculateds_QuaterCalculatedId",
                        column: x => x.QuaterCalculatedId,
                        principalTable: "tbl_QuaterCalculateds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_KPITasks_tbl_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "tbl_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_KPITasks_tbl_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "tbl_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_UserTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KPITaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_UserTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_UserTasks_tbl_KPITasks_KPITaskId",
                        column: x => x.KPITaskId,
                        principalTable: "tbl_KPITasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_UserTasks_tbl_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "tbl_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_KPITasks_CreatedById",
                table: "tbl_KPITasks",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_KPITasks_DepartmentId",
                table: "tbl_KPITasks",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_KPITasks_FactoryId",
                table: "tbl_KPITasks",
                column: "FactoryId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_KPITasks_FormulaId",
                table: "tbl_KPITasks",
                column: "FormulaId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_KPITasks_OwnerId",
                table: "tbl_KPITasks",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_KPITasks_QuaterCalculatedId",
                table: "tbl_KPITasks",
                column: "QuaterCalculatedId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_UserTasks_KPITaskId",
                table: "tbl_UserTasks",
                column: "KPITaskId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_UserTasks_UserId",
                table: "tbl_UserTasks",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_UserTasks");

            migrationBuilder.DropTable(
                name: "tbl_KPITasks");

            migrationBuilder.DropTable(
                name: "tbl_Formulas");

            migrationBuilder.DropTable(
                name: "tbl_QuaterCalculateds");
        }
    }
}
