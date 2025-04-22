namespace ChordKTV.Dtos.UserActivity;

public class UserQuizResultDto
{
    public Guid QuizId { get; set; }
    public float Score { get; set; }
    public LanguageCode Language { get; set; }
    public DateTime? DateCompleted { get; set; }
    public List<string> CorrectAnswers { get; set; } = [];
}
