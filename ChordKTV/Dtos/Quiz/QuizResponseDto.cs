namespace ChordKTV.Dtos.Quiz
{
    public record QuizQuestionDto(
        int QuestionNumber,
        string LyricPhrase,
        List<string> Options,
        int CorrectOptionIndex
    );

    public record QuizResponseDto(
        Guid QuizId,
        Guid SongId,
        int Difficulty,
        DateTime Timestamp,
        List<QuizQuestionDto> Questions
    );
}
