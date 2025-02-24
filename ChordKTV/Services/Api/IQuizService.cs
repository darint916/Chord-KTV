using ChordKTV.Dtos.Quiz;
using System.Threading.Tasks;

namespace ChordKTV.Services.Api
{
    public interface IQuizService
    {
        Task<QuizResponseDto> GenerateQuizAsync(int geniusId, bool useCachedQuiz, int difficulty, int numQuestions);
    }
}
