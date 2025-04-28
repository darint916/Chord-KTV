namespace ChordKTV.Dtos.Quiz;

using System.ComponentModel.DataAnnotations;

public class QuizRequestDto
{
    public required Guid SongId { get; set; }
    public bool UseCachedQuiz { get; set; }

    [Range(1, 5)]
    public int Difficulty { get; set; } = 3;

    [Range(1, 20)]
    public int NumQuestions { get; set; } = 5;
}
