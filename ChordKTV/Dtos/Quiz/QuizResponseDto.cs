namespace ChordKTV.Dtos.Quiz;

public class QuizQuestionDto
{
    public int QuestionNumber { get; set; }
    public string? StartTimestamp { get; set; }
    public string? EndTimestamp { get; set; }
    public string LyricPhrase { get; set; } = string.Empty;
    public List<string> Options { get; set; } = [];
    public int? CorrectOptionIndex { get; set; }
}

public class QuizResponseDto
{
    public Guid QuizId { get; set; }
    public Guid SongId { get; set; }
    public int Difficulty { get; set; }
    public DateTime Timestamp { get; set; }
    public List<QuizQuestionDto> Questions { get; set; } = [];
}
