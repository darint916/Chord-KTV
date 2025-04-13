using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChordKTV.Migrations
{
    /// <inheritdoc />
    public partial class UpdateHandwritingResultWordTested : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserQuizResults_Quizzes_QuizId",
                table: "UserQuizResults");

            migrationBuilder.DropForeignKey(
                name: "FK_UserQuizResults_Users_UserId",
                table: "UserQuizResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserQuizResults",
                table: "UserQuizResults");

            migrationBuilder.DropColumn(
                name: "FavoriteAlbums",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FavoriteArtists",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FavoriteSongs",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "WordsTested",
                table: "UserHandwritingResults");

            migrationBuilder.RenameTable(
                name: "UserQuizResults",
                newName: "UserQuizResult");

            migrationBuilder.RenameIndex(
                name: "IX_UserQuizResults_UserId",
                table: "UserQuizResult",
                newName: "IX_UserQuizResult_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserQuizResults_QuizId",
                table: "UserQuizResult",
                newName: "IX_UserQuizResult_QuizId");

            migrationBuilder.AddColumn<string>(
                name: "WordTested",
                table: "UserHandwritingResults",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserQuizResult",
                table: "UserQuizResult",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "FavoriteSongs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SongId = table.Column<Guid>(type: "uuid", nullable: false),
                    FavoritedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoriteSongs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FavoriteSongs_Songs_SongId",
                        column: x => x.SongId,
                        principalTable: "Songs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FavoriteSongs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteSongs_SongId",
                table: "FavoriteSongs",
                column: "SongId");

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteSongs_UserId",
                table: "FavoriteSongs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserQuizResult_Quizzes_QuizId",
                table: "UserQuizResult",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserQuizResult_Users_UserId",
                table: "UserQuizResult",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserQuizResult_Quizzes_QuizId",
                table: "UserQuizResult");

            migrationBuilder.DropForeignKey(
                name: "FK_UserQuizResult_Users_UserId",
                table: "UserQuizResult");

            migrationBuilder.DropTable(
                name: "FavoriteSongs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserQuizResult",
                table: "UserQuizResult");

            migrationBuilder.DropColumn(
                name: "WordTested",
                table: "UserHandwritingResults");

            migrationBuilder.RenameTable(
                name: "UserQuizResult",
                newName: "UserQuizResults");

            migrationBuilder.RenameIndex(
                name: "IX_UserQuizResult_UserId",
                table: "UserQuizResults",
                newName: "IX_UserQuizResults_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserQuizResult_QuizId",
                table: "UserQuizResults",
                newName: "IX_UserQuizResults_QuizId");

            migrationBuilder.AddColumn<List<string>>(
                name: "FavoriteAlbums",
                table: "Users",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<List<string>>(
                name: "FavoriteArtists",
                table: "Users",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<List<string>>(
                name: "FavoriteSongs",
                table: "Users",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<List<string>>(
                name: "WordsTested",
                table: "UserHandwritingResults",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserQuizResults",
                table: "UserQuizResults",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserQuizResults_Quizzes_QuizId",
                table: "UserQuizResults",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserQuizResults_Users_UserId",
                table: "UserQuizResults",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
