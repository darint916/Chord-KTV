using ChordKTV.Data.Api.QuizData;
using ChordKTV.Models.Quiz;
using Microsoft.EntityFrameworkCore;
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
            _logger.LogDebug("Searching for latest quiz with SongId={SongId}, Difficulty={Difficulty}", songId, difficulty);

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

            // Log question and option counts for debugging
            if (quiz.Questions != null && quiz.Questions.Count > 0)
            {
                foreach (QuizQuestion question in quiz.Questions)
                {
                    _logger.LogDebug("Question {QuestionNumber} has {Count} options",
                        question.QuestionNumber, question.Options?.Count ?? 0);
                }
            }

            return quiz;
        }

        public async Task AddAsync(Quiz quiz)
        {
            _logger.LogDebug("Adding quiz with ID={QuizId}, Timestamp={Timestamp}, Questions count={QuestionsCount}",
                quiz.Id, quiz.Timestamp, quiz.Questions?.Count ?? 0);

            // Ensure the timestamp is set if not already
            if (quiz.Timestamp == default)
            {
                quiz.Timestamp = DateTime.UtcNow;
                _logger.LogDebug("Quiz had default timestamp, updated to current UTC time: {Timestamp}",
                    quiz.Timestamp);
            }

            // Get latest quiz timestamp to ensure proper ordering
            DateTime latestTimestamp = DateTime.MinValue;
            Quiz? existingQuiz = await _context.Quizzes
                .Where(q => q.SongId == quiz.SongId && q.Difficulty == quiz.Difficulty)
                .OrderByDescending(q => q.Timestamp)
                .FirstOrDefaultAsync();

            if (existingQuiz != null)
            {
                latestTimestamp = existingQuiz.Timestamp;
                _logger.LogDebug("Found existing quiz with timestamp: {Timestamp}", latestTimestamp);

                // If the new quiz timestamp is not newer than the latest existing quiz,
                // update it to be slightly newer to ensure it appears as the most recent
                if (quiz.Timestamp <= latestTimestamp)
                {
                    // Add 1 second to the latest timestamp to ensure proper ordering
                    quiz.Timestamp = latestTimestamp.AddSeconds(1);
                    _logger.LogDebug("Updated quiz timestamp to be newer than existing: {NewTimestamp}",
                        quiz.Timestamp);
                }
            }

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            _logger.LogDebug("Successfully saved quiz with ID={QuizId}, Timestamp={Timestamp}",
                quiz.Id, quiz.Timestamp);
        }
    }
}
