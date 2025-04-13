using System.ComponentModel.DataAnnotations;
using ChordKTV.Models.UserData;

namespace ChordKTV.Models.Handwriting;

public class UserHandwritingResult
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Language { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public string WordTested { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
    public Guid UserId { get; set; }
} 