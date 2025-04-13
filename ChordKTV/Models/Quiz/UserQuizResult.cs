using System.ComponentModel.DataAnnotations;
using ChordKTV.Models.UserData;

namespace ChordKTV.Models.Quiz;

public class UserQuizResult
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;
    public decimal Score { get; set; }
    public string Language { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
    public Guid UserId { get; set; }
}
