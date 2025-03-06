using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChordKTV.Migrations
{
    /// <inheritdoc />
    public partial class YoutubeUrlToIDRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "YoutubeUrl",
                table: "Songs",
                newName: "YoutubeId");

            migrationBuilder.RenameColumn(
                name: "AlternateYoutubeUrls",
                table: "Songs",
                newName: "AlternateYoutubeIds");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "YoutubeId",
                table: "Songs",
                newName: "YoutubeUrl");

            migrationBuilder.RenameColumn(
                name: "AlternateYoutubeIds",
                table: "Songs",
                newName: "AlternateYoutubeUrls");
        }
    }
}
