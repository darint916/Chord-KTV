using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChordKTV.Migrations
{
    /// <inheritdoc />
    public partial class PlaylistRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PlaylistUrl",
                table: "UserPlaylistActivities",
                newName: "PlaylistThumbnailUrl");

            migrationBuilder.AddColumn<string>(
                name: "PlaylistId",
                table: "UserPlaylistActivities",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_UserSongActivities_SongId",
                table: "UserSongActivities",
                column: "SongId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSongActivities_Songs_SongId",
                table: "UserSongActivities",
                column: "SongId",
                principalTable: "Songs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSongActivities_Songs_SongId",
                table: "UserSongActivities");

            migrationBuilder.DropIndex(
                name: "IX_UserSongActivities_SongId",
                table: "UserSongActivities");

            migrationBuilder.DropColumn(
                name: "PlaylistId",
                table: "UserPlaylistActivities");

            migrationBuilder.RenameColumn(
                name: "PlaylistThumbnailUrl",
                table: "UserPlaylistActivities",
                newName: "PlaylistUrl");
        }
    }
}
