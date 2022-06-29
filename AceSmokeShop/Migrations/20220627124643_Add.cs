using Microsoft.EntityFrameworkCore.Migrations;

namespace AceSmokeShop.Migrations
{
    public partial class Add : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Stores",
                table: "AspNetUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Stores",
                table: "AspNetUsers",
                type: "nvarchar(256)",
                nullable: true);
        }
    }
}
