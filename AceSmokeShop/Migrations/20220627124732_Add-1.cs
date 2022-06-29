using Microsoft.EntityFrameworkCore.Migrations;

namespace AceSmokeShop.Migrations
{
    public partial class Add1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Stores",
                table: "AspNetUsers",
                type: "nvarchar(256)",
                defaultValue: "None",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Stores",
                table: "AspNetUsers");
        }
    }
}
