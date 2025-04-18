using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChordKTV.Migrations
{
    /// <inheritdoc />
    public partial class TableMerge2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSongActivities_Songs_SongId",
                table: "UserSongActivities");

            migrationBuilder.DropIndex(
                name: "IX_UserSongActivities_SongId",
                table: "UserSongActivities");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
