using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChordKTV.Migrations
{
    /// <inheritdoc />
    public partial class TableMerge1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FavoritePlaylists");

            migrationBuilder.DropTable(
                name: "FavoriteSongs");

            migrationBuilder.DropTable(
                name: "UserSongPlays");

            migrationBuilder.DropColumn(
                name: "LastPlayed",
                table: "UserPlaylistActivities");

            migrationBuilder.DropColumn(
                name: "PlayCount",
                table: "UserPlaylistActivities");

            migrationBuilder.AddColumn<DateTime[]>(
                name: "DatesPlayed",
                table: "UserPlaylistActivities",
                type: "timestamp with time zone[]",
                nullable: false,
                defaultValue: Array.Empty<DateTime>());

            migrationBuilder.AddColumn<bool>(
                name: "IsFavorite",
                table: "UserPlaylistActivities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "UserSongActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SongId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DatesPlayed = table.Column<List<DateTime>>(type: "timestamp with time zone[]", nullable: false),
                    IsFavorite = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSongActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSongActivities_Songs_SongId",
                        column: x => x.SongId,
                        principalTable: "Songs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSongActivities_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSongActivities_SongId",
                table: "UserSongActivities",
                column: "SongId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSongActivities_UserId",
                table: "UserSongActivities",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSongActivities");

            migrationBuilder.DropColumn(
                name: "DatesPlayed",
                table: "UserPlaylistActivities");

            migrationBuilder.DropColumn(
                name: "IsFavorite",
                table: "UserPlaylistActivities");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPlayed",
                table: "UserPlaylistActivities",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "PlayCount",
                table: "UserPlaylistActivities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "FavoritePlaylists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DateFavorited = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlaylistUrl = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoritePlaylists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FavoritePlaylists_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FavoriteSongs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DateFavorited = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SongId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoriteSongs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FavoriteSongs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSongPlays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SongId = table.Column<Guid>(type: "uuid", nullable: false),
                    DatePlayed = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSongPlays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSongPlays_Songs_SongId",
                        column: x => x.SongId,
                        principalTable: "Songs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSongPlays_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FavoritePlaylists_UserId",
                table: "FavoritePlaylists",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteSongs_UserId",
                table: "FavoriteSongs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSongPlays_SongId",
                table: "UserSongPlays",
                column: "SongId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSongPlays_UserId",
                table: "UserSongPlays",
                column: "UserId");
        }
    }
}
