namespace ChordKTV.Dtos.Quiz;

public sealed class QuizQuestionDto
{
    public int QuestionNumber { get; set; }
    public string LyricPhrase { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int CorrectOptionIndex { get; set; }

    public QuizQuestionDto() { }

    public QuizQuestionDto(
        int questionNumber,
        string lyricPhrase,
        List<string> options,
        int correctOptionIndex)
    {
        QuestionNumber = questionNumber;
        LyricPhrase = lyricPhrase;
        Options = options;
        CorrectOptionIndex = correctOptionIndex;
    }
}

public sealed class QuizResponseDto
{
    public Guid QuizId { get; set; }
    public Guid SongId { get; set; }
    public int Difficulty { get; set; }
    public DateTime Timestamp { get; set; }
    public List<QuizQuestionDto> Questions { get; set; } = new();

    public QuizResponseDto() { }

    public QuizResponseDto(
        Guid quizId,
        Guid songId,
        int difficulty,
        DateTime timestamp,
        List<QuizQuestionDto> questions)
    {
        QuizId = quizId;
        SongId = songId;
        Difficulty = difficulty;
        Timestamp = timestamp;
        Questions = questions;
    }
}
