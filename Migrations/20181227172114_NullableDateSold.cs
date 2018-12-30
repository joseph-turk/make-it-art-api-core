using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MakeItArtApi.Migrations
{
    public partial class NullableDateSold : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "SoldOn",
                table: "Artworks",
                nullable: true,
                oldClrType: typeof(DateTime));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "SoldOn",
                table: "Artworks",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);
        }
    }
}
