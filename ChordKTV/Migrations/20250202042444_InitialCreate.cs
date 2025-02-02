using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ChordKTV.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Albums",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                IsSingle = table.Column<bool>(type: "boolean", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Artist = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Albums", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "GeniusMetaData",
            columns: table => new
            {
                GeniusId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                HeaderImageUrl = table.Column<string>(type: "text", nullable: true),
                HeaderImageThumbnailUrl = table.Column<string>(type: "text", nullable: true),
                SongImageUrl = table.Column<string>(type: "text", nullable: true),
                SongImageThumbnailUrl = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GeniusMetaData", x => x.GeniusId);
            });

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Email = table.Column<string>(type: "text", nullable: false),
                FavoriteSongs = table.Column<List<string>>(type: "text[]", nullable: false),
                FavoriteArtists = table.Column<List<string>>(type: "text[]", nullable: false),
                FavoriteAlbums = table.Column<List<string>>(type: "text[]", nullable: false),
                FavoritePlaylistLinks = table.Column<List<string>>(type: "text[]", nullable: false),
                SongHistory = table.Column<List<string>>(type: "text[]", nullable: false),
                PlaylistHistory = table.Column<List<string>>(type: "text[]", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Songs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                AlternateNames = table.Column<List<string>>(type: "text[]", nullable: false),
                PrimaryArtist = table.Column<string>(type: "text", nullable: false),
                FeaturedArtists = table.Column<List<string>>(type: "text[]", nullable: false),
                ReleaseDate = table.Column<DateOnly>(type: "date", nullable: true),
                Genre = table.Column<string>(type: "text", nullable: true),
                SyncLyrics = table.Column<string>(type: "text", nullable: false),
                SongDuration = table.Column<TimeSpan>(type: "interval", nullable: true),
                PlainLyrics = table.Column<string>(type: "text", nullable: false),
                LLMTranslation = table.Column<string>(type: "text", nullable: false),
                GeniusMetaDataGeniusId = table.Column<int>(type: "integer", nullable: false),
                LrcId = table.Column<int>(type: "integer", nullable: false),
                YoutubeUrl = table.Column<string>(type: "text", nullable: false),
                AlternateYoutubeUrls = table.Column<List<string>>(type: "text[]", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Songs", x => x.Id);
                table.ForeignKey(
                    name: "FK_Songs_GeniusMetaData_GeniusMetaDataGeniusId",
                    column: x => x.GeniusMetaDataGeniusId,
                    principalTable: "GeniusMetaData",
                    principalColumn: "GeniusId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AlbumSong",
            columns: table => new
            {
                AlbumsId = table.Column<Guid>(type: "uuid", nullable: false),
                SongsId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AlbumSong", x => new { x.AlbumsId, x.SongsId });
                table.ForeignKey(
                    name: "FK_AlbumSong_Albums_AlbumsId",
                    column: x => x.AlbumsId,
                    principalTable: "Albums",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AlbumSong_Songs_SongsId",
                    column: x => x.SongsId,
                    principalTable: "Songs",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AlbumSong_SongsId",
            table: "AlbumSong",
            column: "SongsId");

        migrationBuilder.CreateIndex(
            name: "IX_Songs_GeniusMetaDataGeniusId",
            table: "Songs",
            column: "GeniusMetaDataGeniusId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AlbumSong");

        migrationBuilder.DropTable(
            name: "Users");

        migrationBuilder.DropTable(
            name: "Albums");

        migrationBuilder.DropTable(
            name: "Songs");

        migrationBuilder.DropTable(
            name: "GeniusMetaData");
    }
}
