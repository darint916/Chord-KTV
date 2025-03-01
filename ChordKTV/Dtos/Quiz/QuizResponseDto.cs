namespace ChordKTV.Dtos.Quiz;

public sealed class QuizQuestionDto
{
    public int QuestionNumber { get; set; }
    public string LyricPhrase { get; set; } = string.Empty;
    public global::System.Collections.Generic.List<string> Options { get; set; } = new();
    public int CorrectOptionIndex { get; set; }
    
    public QuizQuestionDto() { }
    
    public QuizQuestionDto(
        int questionNumber,
        string lyricPhrase,
        global::System.Collections.Generic.List<string> options,
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
    public global::System.Guid QuizId { get; set; }
    public global::System.Guid SongId { get; set; }
    public int Difficulty { get; set; }
    public global::System.DateTime Timestamp { get; set; }
    public global::System.Collections.Generic.List<QuizQuestionDto> Questions { get; set; } = new();
    
    public QuizResponseDto() { }
    
    public QuizResponseDto(
        global::System.Guid quizId,
        global::System.Guid songId,
        int difficulty,
        global::System.DateTime timestamp,
        global::System.Collections.Generic.List<QuizQuestionDto> questions)
    {
        QuizId = quizId;
        SongId = songId;
        Difficulty = difficulty;
        Timestamp = timestamp;
        Questions = questions;
    }
}
