using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    /// <inheritdoc />
    public partial class Update_db_version1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Users_tbl_Groups_GroupId",
                table: "tbl_Users");

            migrationBuilder.DropIndex(
                name: "IX_tbl_Users_GroupId",
                table: "tbl_Users");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "tbl_Users",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "tbl_Positions",
                type: "nvarchar(2400)",
                maxLength: 2400,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DesCription",
                table: "tbl_Positions",
                type: "nvarchar(2400)",
                maxLength: 2400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "tbl_Positions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "tbl_Factorys",
                type: "nvarchar(2400)",
                maxLength: 2400,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "tbl_Factorys",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FactoryType",
                table: "tbl_Factorys",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "tbl_Departments",
                type: "nvarchar(2400)",
                maxLength: 2400,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DepartmentType",
                table: "tbl_Departments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DesCription",
                table: "tbl_Departments",
                type: "nvarchar(2400)",
                maxLength: 2400,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Users_DepartmentId",
                table: "tbl_Users",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Users_FactoryId",
                table: "tbl_Users",
                column: "FactoryId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Users_PositionId",
                table: "tbl_Users",
                column: "PositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Users_tbl_Departments_DepartmentId",
                table: "tbl_Users",
                column: "DepartmentId",
                principalTable: "tbl_Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Users_tbl_Factorys_FactoryId",
                table: "tbl_Users",
                column: "FactoryId",
                principalTable: "tbl_Factorys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Users_tbl_Positions_PositionId",
                table: "tbl_Users",
                column: "PositionId",
                principalTable: "tbl_Positions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Users_tbl_Departments_DepartmentId",
                table: "tbl_Users");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Users_tbl_Factorys_FactoryId",
                table: "tbl_Users");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Users_tbl_Positions_PositionId",
                table: "tbl_Users");

            migrationBuilder.DropIndex(
                name: "IX_tbl_Users_DepartmentId",
                table: "tbl_Users");

            migrationBuilder.DropIndex(
                name: "IX_tbl_Users_FactoryId",
                table: "tbl_Users");

            migrationBuilder.DropIndex(
                name: "IX_tbl_Users_PositionId",
                table: "tbl_Users");

            migrationBuilder.DropColumn(
                name: "DesCription",
                table: "tbl_Positions");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "tbl_Positions");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "tbl_Factorys");

            migrationBuilder.DropColumn(
                name: "FactoryType",
                table: "tbl_Factorys");

            migrationBuilder.DropColumn(
                name: "DepartmentType",
                table: "tbl_Departments");

            migrationBuilder.DropColumn(
                name: "DesCription",
                table: "tbl_Departments");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "tbl_Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "tbl_Positions",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2400)",
                oldMaxLength: 2400,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "tbl_Factorys",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2400)",
                oldMaxLength: 2400,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "tbl_Departments",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2400)",
                oldMaxLength: 2400,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Users_GroupId",
                table: "tbl_Users",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Users_tbl_Groups_GroupId",
                table: "tbl_Users",
                column: "GroupId",
                principalTable: "tbl_Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
