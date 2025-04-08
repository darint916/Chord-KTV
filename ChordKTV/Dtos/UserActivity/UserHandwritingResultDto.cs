namespace ChordKTV.Dtos.UserActivity;

public class UserHandwritingResultDto
{
    public string Language { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public List<string> WordsTested { get; set; } = [];
    public DateTime CompletedAt { get; set; }
} 