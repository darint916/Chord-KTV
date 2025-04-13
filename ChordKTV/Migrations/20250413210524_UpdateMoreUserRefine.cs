using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChordKTV.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMoreUserRefine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteSongs_Songs_SongId",
                table: "FavoriteSongs");

            migrationBuilder.DropIndex(
                name: "IX_FavoriteSongs_SongId",
                table: "FavoriteSongs");

            migrationBuilder.DropColumn(
                name: "FavoritePlaylistLinks",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PlaylistHistory",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SongHistory",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "FavoritePlaylistLinks",
                table: "Users",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<List<string>>(
                name: "PlaylistHistory",
                table: "Users",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<List<string>>(
                name: "SongHistory",
                table: "Users",
                type: "text[]",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteSongs_SongId",
                table: "FavoriteSongs",
                column: "SongId");

            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteSongs_Songs_SongId",
                table: "FavoriteSongs",
                column: "SongId",
                principalTable: "Songs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
