using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChordKTV.Migrations
{
    /// <inheritdoc />
    public partial class SongRefactorAddRomanizedLyrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SyncLyrics",
                table: "Songs",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "SongDuration",
                table: "Songs",
                newName: "Duration");

            migrationBuilder.RenameColumn(
                name: "PrimaryArtist",
                table: "Songs",
                newName: "LrcTranslatedLyrics");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Songs",
                newName: "LrcRomanizedLyrics");

            migrationBuilder.RenameColumn(
                name: "LLMTranslation",
                table: "Songs",
                newName: "LrcLyrics");

            migrationBuilder.RenameColumn(
                name: "AlternateNames",
                table: "Songs",
                newName: "AlternateTitles");

            migrationBuilder.AddColumn<string>(
                name: "Artist",
                table: "Songs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RomLrcId",
                table: "Songs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Artist",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "RomLrcId",
                table: "Songs");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Songs",
                newName: "SyncLyrics");

            migrationBuilder.RenameColumn(
                name: "LrcTranslatedLyrics",
                table: "Songs",
                newName: "PrimaryArtist");

            migrationBuilder.RenameColumn(
                name: "LrcRomanizedLyrics",
                table: "Songs",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "LrcLyrics",
                table: "Songs",
                newName: "LLMTranslation");

            migrationBuilder.RenameColumn(
                name: "Duration",
                table: "Songs",
                newName: "SongDuration");

            migrationBuilder.RenameColumn(
                name: "AlternateTitles",
                table: "Songs",
                newName: "AlternateNames");
        }
    }
}
