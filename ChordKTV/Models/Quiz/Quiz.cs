using System;

namespace ChordKTV.Models.Quiz;

public sealed class Quiz
{
    public global::System.Guid Id { get; set; } = global::System.Guid.NewGuid();
    public global::System.Guid SongId { get; set; }
    public global::System.Collections.Generic.List<QuizQuestion> Questions { get; set; } = [];
    public global::System.DateTime Timestamp { get; set; } = global::System.DateTime.UtcNow;
    public int Difficulty { get; set; }
    public int NumQuestions { get; set; }
}
