using System.ComponentModel.DataAnnotations;
using ChordKTV.Models.UserData;
using ChordKTV.Dtos;

namespace ChordKTV.Models.Handwriting;

public class LearnedWord
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Word { get; set; }
    public LanguageCode Language { get; set; }
    public DateTime DateLearned { get; set; }
    public Guid UserId { get; set; }
}
