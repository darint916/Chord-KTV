namespace ChordKTV.Models.Quiz;

public sealed class QuizOption
{
    public global::System.Guid Id { get; set; } = global::System.Guid.NewGuid();
    public global::System.Guid QuestionId { get; set; }
    public QuizQuestion Question { get; set; } = null!;
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int OrderIndex { get; set; }
} 