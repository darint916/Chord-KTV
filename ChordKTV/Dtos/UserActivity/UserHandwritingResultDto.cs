namespace ChordKTV.Dtos.UserActivity;

public class UserHandwritingResultDto
{
    public LanguageCode Language { get; set; }
    public float Score { get; set; }
    public string WordTested { get; set; } = string.Empty;
    public DateTime? DateCompleted { get; set; }
}
