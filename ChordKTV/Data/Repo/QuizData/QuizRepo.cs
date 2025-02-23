using ChordKTV.Data.Api.QuizData;
using ChordKTV.Models.Quiz;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ChordKTV.Data.Repo.QuizData
{
    public class QuizRepo : IQuizRepo
    {
        private readonly AppDbContext _context;
        private readonly ILogger<QuizRepo> _logger;

        public QuizRepo(AppDbContext context, ILogger<QuizRepo> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Quiz?> GetLatestQuizAsync(int geniusId, int difficulty)
        {
            return await _context.Quizzes
                .Where(q => q.GeniusId == geniusId && q.Difficulty == difficulty)
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