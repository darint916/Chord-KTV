using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1711

namespace ChordKTV.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLanguageFieldsToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- Convert LearnedWords table
                ALTER TABLE ""LearnedWords"" 
                ALTER COLUMN ""Language"" TYPE integer 
                USING CASE 
                    WHEN ""Language"" = 'UNK' THEN 0
                    WHEN ""Language"" = 'AF' THEN 1
                    WHEN ""Language"" = 'AR' THEN 2
                    WHEN ""Language"" = 'BG' THEN 3
                    WHEN ""Language"" = 'BN' THEN 4
                    WHEN ""Language"" = 'CA' THEN 5
                    WHEN ""Language"" = 'CS' THEN 6
                    WHEN ""Language"" = 'DA' THEN 7
                    WHEN ""Language"" = 'DE' THEN 8
                    WHEN ""Language"" = 'EL' THEN 9
                    WHEN ""Language"" = 'EN' THEN 10
                    WHEN ""Language"" = 'ES' THEN 11
                    WHEN ""Language"" = 'ET' THEN 12
                    WHEN ""Language"" = 'FA' THEN 13
                    WHEN ""Language"" = 'FI' THEN 14
                    WHEN ""Language"" = 'FR' THEN 15
                    WHEN ""Language"" = 'GU' THEN 16
                    WHEN ""Language"" = 'HE' THEN 17
                    WHEN ""Language"" = 'HI' THEN 18
                    WHEN ""Language"" = 'HR' THEN 19
                    WHEN ""Language"" = 'HU' THEN 20
                    WHEN ""Language"" = 'ID' THEN 21
                    WHEN ""Language"" = 'IT' THEN 22
                    WHEN ""Language"" = 'JA' THEN 23
                    WHEN ""Language"" = 'KO' THEN 24
                    WHEN ""Language"" = 'LT' THEN 25
                    WHEN ""Language"" = 'LV' THEN 26
                    WHEN ""Language"" = 'MS' THEN 27
                    WHEN ""Language"" = 'NL' THEN 28
                    WHEN ""Language"" = 'NO' THEN 29
                    WHEN ""Language"" = 'PL' THEN 30
                    WHEN ""Language"" = 'PT' THEN 31
                    WHEN ""Language"" = 'RO' THEN 32
                    WHEN ""Language"" = 'RU' THEN 33
                    WHEN ""Language"" = 'SK' THEN 34
                    WHEN ""Language"" = 'SL' THEN 35
                    WHEN ""Language"" = 'SR' THEN 36
                    WHEN ""Language"" = 'SV' THEN 37
                    WHEN ""Language"" = 'TA' THEN 38
                    WHEN ""Language"" = 'TE' THEN 39
                    WHEN ""Language"" = 'TH' THEN 40
                    WHEN ""Language"" = 'TR' THEN 41
                    WHEN ""Language"" = 'UK' THEN 42
                    WHEN ""Language"" = 'VI' THEN 43
                    WHEN ""Language"" = 'ZH' THEN 44
                    ELSE 0 END;

                -- Convert UserQuizzesDone table
                ALTER TABLE ""UserQuizzesDone"" 
                ALTER COLUMN ""Language"" TYPE integer 
                USING CASE 
                    -- Same case mapping as above
                    WHEN ""Language"" = 'UNK' THEN 0
                    WHEN ""Language"" = 'AF' THEN 1
                    -- Include all the same mappings as above
                    ELSE 0 END;

                -- Convert UserHandwritingResults table
                ALTER TABLE ""UserHandwritingResults"" 
                ALTER COLUMN ""Language"" TYPE integer 
                USING CASE 
                    -- Same case mapping as above
                    WHEN ""Language"" = 'UNK' THEN 0
                    WHEN ""Language"" = 'AF' THEN 1
                    -- Include all the same mappings as above
                    ELSE 0 END;
            ");
            migrationBuilder.RenameColumn(
                name: "PlayedAt",
                table: "UserSongPlays",
                newName: "DatePlayed");

            migrationBuilder.RenameColumn(
                name: "CompletedAt",
                table: "UserQuizzesDone",
                newName: "DateCompleted");

            migrationBuilder.RenameColumn(
                name: "CompletedAt",
                table: "UserHandwritingResults",
                newName: "DateCompleted");

            migrationBuilder.RenameColumn(
                name: "LearnedOn",
                table: "LearnedWords",
                newName: "DateLearned");

            migrationBuilder.RenameColumn(
                name: "FavoritedAt",
                table: "FavoriteSongs",
                newName: "DateFavorited");

            migrationBuilder.RenameColumn(
                name: "FavoritedAt",
                table: "FavoritePlaylists",
                newName: "DateFavorited");

            migrationBuilder.AlterColumn<float>(
                name: "Score",
                table: "UserQuizzesDone",
                type: "real",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<int>(
                name: "Language",
                table: "UserQuizzesDone",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<float>(
                name: "Score",
                table: "UserHandwritingResults",
                type: "real",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<int>(
                name: "Language",
                table: "UserHandwritingResults",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Language",
                table: "LearnedWords",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DatePlayed",
                table: "UserSongPlays",
                newName: "PlayedAt");

            migrationBuilder.RenameColumn(
                name: "DateCompleted",
                table: "UserQuizzesDone",
                newName: "CompletedAt");

            migrationBuilder.RenameColumn(
                name: "DateCompleted",
                table: "UserHandwritingResults",
                newName: "CompletedAt");

            migrationBuilder.RenameColumn(
                name: "DateLearned",
                table: "LearnedWords",
                newName: "LearnedOn");

            migrationBuilder.RenameColumn(
                name: "DateFavorited",
                table: "FavoriteSongs",
                newName: "FavoritedAt");

            migrationBuilder.RenameColumn(
                name: "DateFavorited",
                table: "FavoritePlaylists",
                newName: "FavoritedAt");

            migrationBuilder.AlterColumn<decimal>(
                name: "Score",
                table: "UserQuizzesDone",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<string>(
                name: "Language",
                table: "UserQuizzesDone",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "Score",
                table: "UserHandwritingResults",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<string>(
                name: "Language",
                table: "UserHandwritingResults",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Language",
                table: "LearnedWords",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
