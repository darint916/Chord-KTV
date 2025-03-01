using ChordKTV.Models.Quiz;
using System;
using System.Threading.Tasks;

namespace ChordKTV.Data.Api.QuizData
{
    public interface IQuizRepo
    {
        public Task<Quiz?> GetLatestQuizAsync(Guid songId, int difficulty);
        public Task AddAsync(Quiz quiz);
    }
}
