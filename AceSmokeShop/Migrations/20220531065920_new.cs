using Microsoft.EntityFrameworkCore.Migrations;

namespace AceSmokeShop.Migrations
{
    public partial class @new : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbl_product",
                columns: table => new
                {
                    ProductID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Barcode = table.Column<string>(type: "nvarchar(256)", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(256)", nullable: false),
                    CategoryID = table.Column<int>(type: "int", nullable: false),
                    SubCategoryID = table.Column<int>(type: "int", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    BasePrice = table.Column<decimal>(type: "decimal(35,2)", nullable: false),
                    SalePrice = table.Column<decimal>(type: "decimal(35,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    IsPromoted = table.Column<bool>(type: "bit", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    PrimaryImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecondaryImage1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecondaryImage2 = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_product", x => x.ProductID);
                    table.ForeignKey(
                        name: "FK_tbl_product_tbl_category_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "tbl_category",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_product_tbl_subcategory_SubCategoryID",
                        column: x => x.SubCategoryID,
                        principalTable: "tbl_subcategory",
                        principalColumn: "SubCategoryID",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "tbl_userorders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustOrderId = table.Column<string>(type: "nvarchar(256)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ShippingAddressId = table.Column<int>(type: "int", nullable: false),
                    BillingAddressId = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(35,2)", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(35,2)", nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(35,2)", nullable: false),
                    ShippingCharge = table.Column<decimal>(type: "decimal(35,2)", nullable: false),
                    OrderStatus = table.Column<string>(type: "nvarchar(256)", nullable: false),
                    PaymentId = table.Column<string>(type: "nvarchar(256)", nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(256)", nullable: false),
                    CreateDate = table.Column<string>(type: "nvarchar(256)", nullable: false),
                    DeliveryDate = table.Column<string>(type: "nvarchar(256)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_userorders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_userorders_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_userorders_tbl_addresses_BillingAddressId",
                        column: x => x.BillingAddressId,
                        principalTable: "tbl_addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_userorders_tbl_addresses_ShippingAddressId",
                        column: x => x.ShippingAddressId,
                        principalTable: "tbl_addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "tbl_cart",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_cart", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_cart_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_cart_tbl_product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "tbl_product",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_orderitem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    CustOrderId = table.Column<string>(type: "nvarchar(256)", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(35,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UserOrderId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_orderitem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_orderitem_tbl_product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "tbl_product",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_orderitem_tbl_userorders_UserOrderId",
                        column: x => x.UserOrderId,
                        principalTable: "tbl_userorders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_ordershipstatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<string>(type: "nvarchar(256)", nullable: false),
                    CustOrderId = table.Column<string>(type: "nvarchar(256)", nullable: false),
                    CreateDate = table.Column<string>(type: "nvarchar(256)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    UserOrdersId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ordershipstatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ordershipstatus_tbl_userorders_UserOrdersId",
                        column: x => x.UserOrdersId,
                        principalTable: "tbl_userorders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_cart_ProductId",
                table: "tbl_cart",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_cart_UserId",
                table: "tbl_cart",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_orderitem_ProductId",
                table: "tbl_orderitem",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_orderitem_UserOrderId",
                table: "tbl_orderitem",
                column: "UserOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ordershipstatus_UserOrdersId",
                table: "tbl_ordershipstatus",
                column: "UserOrdersId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_product_CategoryID",
                table: "tbl_product",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_product_SubCategoryID",
                table: "tbl_product",
                column: "SubCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_userorders_BillingAddressId",
                table: "tbl_userorders",
                column: "BillingAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_userorders_ShippingAddressId",
                table: "tbl_userorders",
                column: "ShippingAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_userorders_UserId",
                table: "tbl_userorders",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_cart");

            migrationBuilder.DropTable(
                name: "tbl_orderitem");

            migrationBuilder.DropTable(
                name: "tbl_ordershipstatus");

            migrationBuilder.DropTable(
                name: "tbl_product");

            migrationBuilder.DropTable(
                name: "tbl_userorders");
        }
    }
}
