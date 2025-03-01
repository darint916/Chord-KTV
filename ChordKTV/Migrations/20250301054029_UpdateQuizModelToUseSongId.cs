using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChordKTV.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuizModelToUseSongId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GeniusId",
                table: "Quizzes");

            migrationBuilder.AddColumn<Guid>(
                name: "SongId",
                table: "Quizzes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SongId",
                table: "Quizzes");

            migrationBuilder.AddColumn<int>(
                name: "GeniusId",
                table: "Quizzes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
