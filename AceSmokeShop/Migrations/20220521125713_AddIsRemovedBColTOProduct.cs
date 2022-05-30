using Microsoft.EntityFrameworkCore.Migrations;

namespace AceSmokeShop.Migrations
{
    public partial class AddIsRemovedBColTOProduct : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRemoved",
                table: "tbl_product",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "tbl_addresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AddressLineA = table.Column<string>(type: "NVARCHAR(256)", nullable: false),
                    AddressLineB = table.Column<string>(type: "NVARCHAR(256)", nullable: true),
                    City = table.Column<string>(type: "NVARCHAR(256)", nullable: false),
                    StateID = table.Column<int>(type: "int", nullable: false),
                    Zipcode = table.Column<int>(type: "int", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    UserId1 = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_addresses_AspNetUsers_UserId1",
                        column: x => x.UserId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_addresses_tbl_state_StateID",
                        column: x => x.StateID,
                        principalTable: "tbl_state",
                        principalColumn: "StateID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_addresses_StateID",
                table: "tbl_addresses",
                column: "StateID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_addresses_UserId1",
                table: "tbl_addresses",
                column: "UserId1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_addresses");

            migrationBuilder.DropColumn(
                name: "IsRemoved",
                table: "tbl_product");
        }
    }
}
