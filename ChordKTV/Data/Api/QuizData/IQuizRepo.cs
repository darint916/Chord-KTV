namespace ChordKTV.Data.Api.QuizData;

public interface IQuizRepo
{
    public Task<Models.Quiz.Quiz?> GetLatestQuizAsync(Guid songId, int difficulty);
    public Task AddAsync(Models.Quiz.Quiz quiz);
}
