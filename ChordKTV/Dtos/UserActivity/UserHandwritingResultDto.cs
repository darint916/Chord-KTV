namespace ChordKTV.Dtos.UserActivity;

public class UserHandwritingResultDto
{
    public string Language { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public string WordTested { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
}
