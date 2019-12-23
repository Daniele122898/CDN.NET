using Microsoft.EntityFrameworkCore.Migrations;

namespace CDN.NET_backend.Migrations
{
    public partial class RemovedAlbumPublicId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Albums");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublicId",
                table: "Albums",
                type: "longtext",
                nullable: false,
                defaultValue: "");
        }
    }
}
