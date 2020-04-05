using Microsoft.EntityFrameworkCore.Migrations;

namespace CDN.NET_backend.Migrations
{
    public partial class AlbumFileSetNull2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UFiles_Albums_AlbumId",
                table: "UFiles");

            migrationBuilder.AddForeignKey(
                name: "FK_UFiles_Albums_AlbumId",
                table: "UFiles",
                column: "AlbumId",
                principalTable: "Albums",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UFiles_Albums_AlbumId",
                table: "UFiles");

            migrationBuilder.AddForeignKey(
                name: "FK_UFiles_Albums_AlbumId",
                table: "UFiles",
                column: "AlbumId",
                principalTable: "Albums",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
