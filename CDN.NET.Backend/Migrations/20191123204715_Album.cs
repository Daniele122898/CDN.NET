using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CDN.NET_backend.Migrations
{
    public partial class Album : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UFiles_Users_OwnerId",
                table: "UFiles");

            migrationBuilder.AddColumn<int>(
                name: "AlbumId",
                table: "UFiles",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Albums",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PublicId = table.Column<string>(nullable: false),
                    IsPublic = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    DateAdded = table.Column<DateTime>(nullable: false),
                    OwnerId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Albums", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Albums_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UFiles_AlbumId",
                table: "UFiles",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_OwnerId",
                table: "Albums",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_UFiles_Albums_AlbumId",
                table: "UFiles",
                column: "AlbumId",
                principalTable: "Albums",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UFiles_Users_OwnerId",
                table: "UFiles",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UFiles_Albums_AlbumId",
                table: "UFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_UFiles_Users_OwnerId",
                table: "UFiles");

            migrationBuilder.DropTable(
                name: "Albums");

            migrationBuilder.DropIndex(
                name: "IX_UFiles_AlbumId",
                table: "UFiles");

            migrationBuilder.DropColumn(
                name: "AlbumId",
                table: "UFiles");

            migrationBuilder.AddForeignKey(
                name: "FK_UFiles_Users_OwnerId",
                table: "UFiles",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
