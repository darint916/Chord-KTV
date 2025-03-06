using ChordKTV.Data.Api.QuizData;
using ChordKTV.Models.Quiz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace ChordKTV.Data.Repo.QuizData
{
    public sealed class QuizRepo : IQuizRepo
    {
        private readonly AppDbContext _context;
        private readonly ILogger<QuizRepo> _logger;

        public QuizRepo(AppDbContext context, ILogger<QuizRepo> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Quiz?> GetLatestQuizAsync(Guid songId, int difficulty)
        {
            // Get the most recent quiz with questions and options in a single query
            Quiz? quiz = await _context.Quizzes
                .Where(q => q.SongId == songId && q.Difficulty == difficulty)
                .OrderByDescending(q => q.Timestamp)
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync();

            if (quiz == null)
            {
                _logger.LogDebug("No quizzes found for SongId={SongId}, Difficulty={Difficulty}", songId, difficulty);
                return null;
            }

            _logger.LogDebug("Found quiz with ID={QuizId}, Timestamp={Timestamp}, Questions count={QuestionsCount}",
                quiz.Id, quiz.Timestamp, quiz.Questions?.Count ?? 0);

            return quiz;
        }

        public async Task AddAsync(Quiz quiz)
        {
            // Always use current time for consistency
            quiz.Timestamp = DateTime.UtcNow;

            // Use a transaction to ensure consistency in concurrent scenarios
            using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Add the quiz to the context
                _context.Quizzes.Add(quiz);

                // Persist to database
                await _context.SaveChangesAsync();

                // Commit the transaction
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding quiz with ID={QuizId}", quiz.Id);
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
