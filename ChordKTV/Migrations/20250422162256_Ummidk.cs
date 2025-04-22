using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChordKTV.Migrations
{
    /// <inheritdoc />
    public partial class Ummidk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateFavorited",
                table: "UserSongActivities",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPlayed",
                table: "UserSongActivities",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateFavorited",
                table: "UserPlaylistActivities",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPlayed",
                table: "UserPlaylistActivities",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateFavorited",
                table: "UserSongActivities");

            migrationBuilder.DropColumn(
                name: "LastPlayed",
                table: "UserSongActivities");

            migrationBuilder.DropColumn(
                name: "DateFavorited",
                table: "UserPlaylistActivities");

            migrationBuilder.DropColumn(
                name: "LastPlayed",
                table: "UserPlaylistActivities");
        }
    }
}
