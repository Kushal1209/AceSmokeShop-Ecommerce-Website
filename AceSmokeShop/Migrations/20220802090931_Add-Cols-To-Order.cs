using Microsoft.EntityFrameworkCore.Migrations;

namespace AceSmokeShop.Migrations
{
    public partial class AddColsToOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RefundId",
                table: "tbl_userorders",
                type: "nvarchar(256)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VoidId",
                table: "tbl_userorders",
                type: "nvarchar(256)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefundId",
                table: "tbl_transactions",
                type: "nvarchar(256)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VoidId",
                table: "tbl_transactions",
                type: "nvarchar(256)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefundId",
                table: "tbl_userorders");

            migrationBuilder.DropColumn(
                name: "VoidId",
                table: "tbl_userorders");

            migrationBuilder.DropColumn(
                name: "RefundId",
                table: "tbl_transactions");

            migrationBuilder.DropColumn(
                name: "VoidId",
                table: "tbl_transactions");
        }
    }
}
