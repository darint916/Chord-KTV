namespace ChordKTV.Dtos.Quiz;

public sealed record QuizQuestionDto(
    int QuestionNumber,
    string LyricPhrase,
    global::System.Collections.Generic.List<string> Options,
    int CorrectOptionIndex
);

public sealed record QuizResponseDto(
    global::System.Guid QuizId,
    global::System.Guid SongId,
    int Difficulty,
    global::System.DateTime Timestamp,
    global::System.Collections.Generic.List<QuizQuestionDto> Questions
);
