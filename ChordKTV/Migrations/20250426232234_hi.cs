using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

// Ignore IDE1006 warning for this file
#pragma warning disable IDE1006

namespace ChordKTV.Migrations
{
    /// <inheritdoc />
    public partial class hi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "YoutubeInstrumentalId",
                table: "Songs",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "YoutubeInstrumentalId",
                table: "Songs");
        }
    }
}

#pragma warning restore IDE1006
