using System.ComponentModel.DataAnnotations;

namespace ChordKTV.Models.Quiz;

public class Quiz
{
    [Key]
    public Guid Id { get; set; }
    public Guid SongId { get; set; }
    public List<QuizQuestion> Questions { get; set; } = [];
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int Difficulty { get; set; }
    public int NumQuestions { get; set; }
}
