using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AceSmokeShop.Migrations
{
    public partial class DateTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Dob",
                table: "AspNetUsers",
                type: "nvarchar(256)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "smalldatetime");

            migrationBuilder.AddColumn<string>(
                name: "CreateDate",
                table: "AspNetUsers",
                type: "nvarchar(256)",
                nullable: true,
                defaultValue: ""
                );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "Dob",
                table: "AspNetUsers",
                type: "smalldatetime",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)");

            migrationBuilder.AddColumn<string>(
               name: "CreateDate",
               table: "AspNetUsers",
               type: "nvarchar(256)",
               nullable: true,
               defaultValue: ""
               );
        }
    }
}
