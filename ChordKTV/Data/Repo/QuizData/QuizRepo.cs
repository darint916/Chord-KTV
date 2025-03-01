using ChordKTV.Data.Api.QuizData;
using ChordKTV.Models.Quiz;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ChordKTV.Data.Repo.QuizData
{
    public class QuizRepo : IQuizRepo
    {
        private readonly AppDbContext _context;

        public QuizRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Quiz?> GetLatestQuizAsync(Guid songId, int difficulty)
        {
            return await _context.Quizzes
                .Where(q => q.SongId == songId && q.Difficulty == difficulty)
                .OrderByDescending(q => q.Timestamp)
                .FirstOrDefaultAsync();
        }

        public async Task AddAsync(Quiz quiz)
        {
            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();
        }
    }
}
