using ChordKTV.Models.Quiz;
using System.Threading.Tasks;

namespace ChordKTV.Data.Api.QuizData
{
    public interface IQuizRepo
    {
        Task<Quiz?> GetLatestQuizAsync(int geniusId);
        Task AddAsync(Quiz quiz);
    }
} 