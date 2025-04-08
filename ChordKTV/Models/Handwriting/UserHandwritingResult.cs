using System.ComponentModel.DataAnnotations;
using ChordKTV.Models.UserData;

namespace ChordKTV.Models.Handwriting;

public class UserHandwritingResult
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Language { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public List<string> WordsTested { get; set; } = [];
    public DateTime CompletedAt { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
} 