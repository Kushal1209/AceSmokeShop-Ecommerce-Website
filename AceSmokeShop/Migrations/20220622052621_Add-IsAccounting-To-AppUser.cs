using Microsoft.EntityFrameworkCore.Migrations;

namespace AceSmokeShop.Migrations
{
    public partial class AddIsAccountingToAppUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAccounting",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAccounting",
                table: "AspNetUsers");
        }
    }
}
