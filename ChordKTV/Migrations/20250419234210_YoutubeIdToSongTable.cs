using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChordKTV.Migrations
{
    /// <inheritdoc />
    public partial class YoutubeIdToSongTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_YoutubeSongs_SongId",
                table: "YoutubeSongs",
                column: "SongId");

            migrationBuilder.AddForeignKey(
                name: "FK_YoutubeSongs_Songs_SongId",
                table: "YoutubeSongs",
                column: "SongId",
                principalTable: "Songs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YoutubeSongs_Songs_SongId",
                table: "YoutubeSongs");

            migrationBuilder.DropIndex(
                name: "IX_YoutubeSongs_SongId",
                table: "YoutubeSongs");
        }
    }
}
