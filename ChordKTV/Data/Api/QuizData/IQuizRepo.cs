namespace ChordKTV.Data.Api.QuizData;

public interface IQuizRepo
{
    public global::System.Threading.Tasks.Task<global::ChordKTV.Models.Quiz.Quiz?> GetLatestQuizAsync(global::System.Guid songId, int difficulty);
    public global::System.Threading.Tasks.Task AddAsync(global::ChordKTV.Models.Quiz.Quiz quiz);
}
