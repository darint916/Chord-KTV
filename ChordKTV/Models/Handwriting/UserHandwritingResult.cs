using System.ComponentModel.DataAnnotations;
using ChordKTV.Models.UserData;
using ChordKTV.Dtos;

namespace ChordKTV.Models.Handwriting;

public class UserHandwritingResult
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public LanguageCode Language { get; set; }
    public float Score { get; set; }
    public string WordTested { get; set; } = string.Empty;
    public DateTime DateCompleted { get; set; }
    public Guid UserId { get; set; }
}
