using Microsoft.EntityFrameworkCore.Migrations;

namespace CDN.NET_backend.Migrations
{
    public partial class AddedContentTypeToUFile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "UFiles",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "UFiles");
        }
    }
}
