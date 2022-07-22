using Microsoft.EntityFrameworkCore.Migrations;

namespace AceSmokeShop.Migrations
{
    public partial class AddColumnToSubCat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShippingCost",
                table: "tbl_subcategory",
                type: "nvarchar(256)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingCost",
                table: "tbl_subcategory");
        }
    }
}
