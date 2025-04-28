using ChordKTV.Models.Quiz;

namespace ChordKTV.Services.Api;

public interface IQuizService
{
    public Task<Quiz> GenerateQuizAsync(Guid songId, bool useCachedQuiz, int difficulty, int numQuestions);

    public Task<Quiz> GenerateAudioQuizAsync(Guid songId, bool useCachedQuiz, int difficulty, int numQuestions);
}
