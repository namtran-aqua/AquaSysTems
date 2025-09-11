using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_Db__ITSuport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ManagerId",
                table: "tbl_RequestClinics",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateTable(
                name: "tbl_RequestSuportCategorys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TechnicianId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_RequestSuportCategorys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_RequestSuportCategorys_tbl_Users_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "tbl_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_RequestSuports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestSuportCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequestDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestSolution = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TechnicianId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InProgessDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_RequestSuports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_RequestSuports_tbl_RequestSuportCategorys_RequestSuportCategoryId",
                        column: x => x.RequestSuportCategoryId,
                        principalTable: "tbl_RequestSuportCategorys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_RequestSuports_tbl_Users_RequestBy",
                        column: x => x.RequestBy,
                        principalTable: "tbl_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_RequestSuports_tbl_Users_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "tbl_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestSuportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileExtend = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_Attachments_tbl_RequestSuports_RequestSuportId",
                        column: x => x.RequestSuportId,
                        principalTable: "tbl_RequestSuports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Attachments_RequestSuportId",
                table: "tbl_Attachments",
                column: "RequestSuportId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_RequestSuportCategorys_TechnicianId",
                table: "tbl_RequestSuportCategorys",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_RequestSuports_RequestBy",
                table: "tbl_RequestSuports",
                column: "RequestBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_RequestSuports_RequestSuportCategoryId",
                table: "tbl_RequestSuports",
                column: "RequestSuportCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_RequestSuports_TechnicianId",
                table: "tbl_RequestSuports",
                column: "TechnicianId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_Attachments");

            migrationBuilder.DropTable(
                name: "tbl_RequestSuports");

            migrationBuilder.DropTable(
                name: "tbl_RequestSuportCategorys");

            migrationBuilder.AlterColumn<Guid>(
                name: "ManagerId",
                table: "tbl_RequestClinics",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
