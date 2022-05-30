using Microsoft.EntityFrameworkCore.Migrations;

namespace AceSmokeShop.Migrations
{
    public partial class Add : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_addresses_AspNetUsers_UserId1",
                table: "tbl_addresses");

            migrationBuilder.DropIndex(
                name: "IX_tbl_addresses_UserId1",
                table: "tbl_addresses");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "tbl_addresses");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "tbl_addresses",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_addresses_UserId",
                table: "tbl_addresses",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_addresses_AspNetUsers_UserId",
                table: "tbl_addresses",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_addresses_AspNetUsers_UserId",
                table: "tbl_addresses");

            migrationBuilder.DropIndex(
                name: "IX_tbl_addresses_UserId",
                table: "tbl_addresses");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "tbl_addresses",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "tbl_addresses",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_addresses_UserId1",
                table: "tbl_addresses",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_addresses_AspNetUsers_UserId1",
                table: "tbl_addresses",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
