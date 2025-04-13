namespace ChordKTV.Dtos.UserActivity;

public class UserQuizResultDto
{
    public Guid QuizId { get; set; }
    public decimal Score { get; set; }
    public string Language { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
    public List<string> CorrectAnswers { get; set; } = [];
} 