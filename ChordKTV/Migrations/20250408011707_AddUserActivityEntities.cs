using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChordKTV.Migrations
{
    /// <inheritdoc />
    public partial class AddUserActivityEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LearnedWords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Word = table.Column<string>(type: "text", nullable: false),
                    Language = table.Column<string>(type: "text", nullable: false),
                    LearnedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearnedWords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearnedWords_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserHandwritingResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Language = table.Column<string>(type: "text", nullable: false),
                    Score = table.Column<decimal>(type: "numeric", nullable: false),
                    WordsTested = table.Column<List<string>>(type: "text[]", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHandwritingResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserHandwritingResults_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPlaylistActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlaylistUrl = table.Column<string>(type: "text", nullable: false),
                    PlayCount = table.Column<int>(type: "integer", nullable: false),
                    LastPlayed = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPlaylistActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPlaylistActivities_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserQuizResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuizId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<decimal>(type: "numeric", nullable: false),
                    Language = table.Column<string>(type: "text", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserQuizResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserQuizResults_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserQuizResults_Users_UserId",
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
                    PlayedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                name: "IX_LearnedWords_UserId",
                table: "LearnedWords",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserHandwritingResults_UserId",
                table: "UserHandwritingResults",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPlaylistActivities_UserId",
                table: "UserPlaylistActivities",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizResults_QuizId",
                table: "UserQuizResults",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizResults_UserId",
                table: "UserQuizResults",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LearnedWords");

            migrationBuilder.DropTable(
                name: "UserHandwritingResults");

            migrationBuilder.DropTable(
                name: "UserPlaylistActivities");

            migrationBuilder.DropTable(
                name: "UserQuizResults");

            migrationBuilder.DropTable(
                name: "UserSongPlays");
        }
    }
}
