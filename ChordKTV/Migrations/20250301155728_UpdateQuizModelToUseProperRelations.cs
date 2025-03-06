using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChordKTV.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuizModelToUseProperRelations : Migration
    {
        // Define static readonly field for the columns array to address CA1861
        // Add underscore prefix to follow naming convention (IDE1006)
        // Simplify collection initialization (IDE0300)
        private static readonly string[] _questionOrderColumns = ["QuestionId", "OrderIndex"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuizJson",
                table: "Quizzes");

            migrationBuilder.CreateTable(
                name: "QuizQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuizId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionNumber = table.Column<int>(type: "integer", nullable: false),
                    LyricPhrase = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizQuestions_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizOptions_QuizQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "QuizQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuizOptions_QuestionId_OrderIndex",
                table: "QuizOptions",
                columns: _questionOrderColumns,
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuizQuestions_QuizId",
                table: "QuizQuestions",
                column: "QuizId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuizOptions");

            migrationBuilder.DropTable(
                name: "QuizQuestions");

            migrationBuilder.AddColumn<string>(
                name: "QuizJson",
                table: "Quizzes",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
