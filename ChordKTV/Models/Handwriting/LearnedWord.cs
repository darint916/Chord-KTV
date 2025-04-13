using System.ComponentModel.DataAnnotations;
using ChordKTV.Models.UserData;

namespace ChordKTV.Models.Handwriting;

public class LearnedWord
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Word { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public DateTime LearnedOn { get; set; }
    public Guid UserId { get; set; }
}
