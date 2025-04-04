namespace ChordKTV.Models.Quiz;

public class QuizQuestion
{
    public Guid Id { get; set; }
    public Guid QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;
    public int QuestionNumber { get; set; }
    public string LyricPhrase { get; set; } = string.Empty;
    public List<QuizOption> Options { get; set; } = [];
}
