using ChordKTV.Models.Quiz;

namespace ChordKTV.Data.Api.QuizData
{
    public interface IQuizRepo
    {
        Task<Quiz?> GetLatestQuizAsync(Guid songId, int difficulty);
        Task AddAsync(Quiz quiz);
    }
}
