using ChordKTV.Dtos.Quiz;
using System;

namespace ChordKTV.Services.Api
{
    public interface IQuizService
    {
        public Task<QuizResponseDto> GenerateQuizAsync(Guid songId, bool useCachedQuiz, int difficulty, int numQuestions);
    }
}
