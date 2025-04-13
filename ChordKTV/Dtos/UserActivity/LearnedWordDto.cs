namespace ChordKTV.Dtos.UserActivity;

public record LearnedWordDto(
    string Word,
    string Language,
    DateTime? LearnedOn
); 