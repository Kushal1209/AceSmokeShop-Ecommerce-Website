using Microsoft.EntityFrameworkCore.Migrations;

namespace AceSmokeShop.Migrations
{
    public partial class re : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_orderitem_tbl_userorders_UserOrderId",
                table: "tbl_orderitem");

            migrationBuilder.DropIndex(
                name: "IX_tbl_orderitem_UserOrderId",
                table: "tbl_orderitem");

            migrationBuilder.DropColumn(
                name: "UserOrderId",
                table: "tbl_orderitem");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_orderitem_OrderId",
                table: "tbl_orderitem",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_orderitem_tbl_userorders_OrderId",
                table: "tbl_orderitem",
                column: "OrderId",
                principalTable: "tbl_userorders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_orderitem_tbl_userorders_OrderId",
                table: "tbl_orderitem");

            migrationBuilder.DropIndex(
                name: "IX_tbl_orderitem_OrderId",
                table: "tbl_orderitem");

            migrationBuilder.AddColumn<int>(
                name: "UserOrderId",
                table: "tbl_orderitem",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_orderitem_UserOrderId",
                table: "tbl_orderitem",
                column: "UserOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_orderitem_tbl_userorders_UserOrderId",
                table: "tbl_orderitem",
                column: "UserOrderId",
                principalTable: "tbl_userorders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
