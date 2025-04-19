using System.ComponentModel.DataAnnotations;
using ChordKTV.Models.UserData;
using ChordKTV.Dtos;

namespace ChordKTV.Models.Quiz;

public class UserQuizResult
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;
    public float Score { get; set; }
    public LanguageCode Language { get; set; }
    public DateTime DateCompleted { get; set; }
    public Guid UserId { get; set; }
}
