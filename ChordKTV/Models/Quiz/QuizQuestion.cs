namespace ChordKTV.Models.Quiz;

public sealed class QuizQuestion
{
    public global::System.Guid Id { get; set; } = global::System.Guid.NewGuid();
    public global::System.Guid QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;
    public int QuestionNumber { get; set; }
    public string LyricPhrase { get; set; } = string.Empty;
    public global::System.Collections.Generic.List<QuizOption> Options { get; set; } = [];
} 