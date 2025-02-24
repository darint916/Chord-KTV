using System;

namespace ChordKTV.Models.Quiz
{
    public class Quiz
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int GeniusId { get; set; }
        public string QuizJson { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public int Difficulty { get; set; }
        public int NumQuestions { get; set; }
    }
}
