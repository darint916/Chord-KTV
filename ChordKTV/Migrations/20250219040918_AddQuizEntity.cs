using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChordKTV.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Quizzes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GeniusId = table.Column<int>(type: "integer", nullable: false),
                    QuizJson = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Difficulty = table.Column<int>(type: "integer", nullable: false),
                    NumQuestions = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quizzes", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Quizzes");
        }
    }
}
