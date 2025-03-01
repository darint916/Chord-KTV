using ChordKTV.Models.Quiz;
using System;
using System.Threading.Tasks;

namespace ChordKTV.Data.Api.QuizData
{
    public interface IQuizRepo
    {
        Task<Quiz?> GetLatestQuizAsync(Guid songId, int difficulty);
        Task AddAsync(Quiz quiz);
    }
}
