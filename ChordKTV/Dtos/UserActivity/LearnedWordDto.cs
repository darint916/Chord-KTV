namespace ChordKTV.Dtos.UserActivity;

public record LearnedWordDto(
    string Word,
    LanguageCode Language,
    DateTime? DateLearned
);
