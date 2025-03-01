using ChordKTV.Dtos.Quiz;
using System.Threading.Tasks;
using System;

namespace ChordKTV.Services.Api
{
    public interface IQuizService
    {
        Task<QuizResponseDto> GenerateQuizAsync(Guid songId, bool useCachedQuiz, int difficulty, int numQuestions);
    }
}
