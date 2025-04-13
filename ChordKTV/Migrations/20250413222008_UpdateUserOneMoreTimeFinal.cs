using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChordKTV.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserOneMoreTimeFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserQuizResult_Quizzes_QuizId",
                table: "UserQuizResult");

            migrationBuilder.DropForeignKey(
                name: "FK_UserQuizResult_Users_UserId",
                table: "UserQuizResult");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserQuizResult",
                table: "UserQuizResult");

            migrationBuilder.RenameTable(
                name: "UserQuizResult",
                newName: "UserQuizzesDone");

            migrationBuilder.RenameIndex(
                name: "IX_UserQuizResult_UserId",
                table: "UserQuizzesDone",
                newName: "IX_UserQuizzesDone_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserQuizResult_QuizId",
                table: "UserQuizzesDone",
                newName: "IX_UserQuizzesDone_QuizId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserQuizzesDone",
                table: "UserQuizzesDone",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserQuizzesDone_Quizzes_QuizId",
                table: "UserQuizzesDone",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserQuizzesDone_Users_UserId",
                table: "UserQuizzesDone",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserQuizzesDone_Quizzes_QuizId",
                table: "UserQuizzesDone");

            migrationBuilder.DropForeignKey(
                name: "FK_UserQuizzesDone_Users_UserId",
                table: "UserQuizzesDone");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserQuizzesDone",
                table: "UserQuizzesDone");

            migrationBuilder.RenameTable(
                name: "UserQuizzesDone",
                newName: "UserQuizResult");

            migrationBuilder.RenameIndex(
                name: "IX_UserQuizzesDone_UserId",
                table: "UserQuizResult",
                newName: "IX_UserQuizResult_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserQuizzesDone_QuizId",
                table: "UserQuizResult",
                newName: "IX_UserQuizResult_QuizId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserQuizResult",
                table: "UserQuizResult",
                column: "Id");

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
    }
}
