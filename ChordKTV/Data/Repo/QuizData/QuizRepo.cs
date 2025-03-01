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
            
            // First check how many quizzes match the criteria
            List<Quiz> quizzes = await _context.Quizzes
                .Where(q => q.SongId == songId && q.Difficulty == difficulty)
                .OrderByDescending(q => q.Timestamp)
                .ToListAsync();
                
            if (quizzes.Count == 0)
            {
                _logger.LogDebug("No quizzes found for SongId={SongId}, Difficulty={Difficulty}", songId, difficulty);
                return null;
            }
            
            // Log all matching quizzes to help diagnose timestamp ordering issues
            _logger.LogDebug("Found {Count} quizzes for SongId={SongId}, Difficulty={Difficulty}", 
                quizzes.Count, songId, difficulty);
                
            for (int i = 0; i < quizzes.Count; i++)
            {
                _logger.LogDebug("Quiz {Index}: ID={QuizId}, Timestamp={Timestamp}", 
                    i, quizzes[i].Id, quizzes[i].Timestamp);
            }
            
            // Get the most recent quiz
            Quiz quiz = quizzes.First();
            _logger.LogDebug("Selected most recent quiz with ID={QuizId}, Timestamp={Timestamp}", 
                quiz.Id, quiz.Timestamp);
            
            // Explicitly load the questions
            await _context.Entry(quiz)
                .Collection(q => q.Questions)
                .LoadAsync();
            
            _logger.LogDebug("Loaded {Count} questions for quiz ID={QuizId}", 
                quiz.Questions?.Count ?? 0, quiz.Id);
            
            // Explicitly load options for each question
            if (quiz.Questions != null && quiz.Questions.Count > 0)
            {
                foreach (QuizQuestion question in quiz.Questions)
                {
                    await _context.Entry(question)
                        .Collection(q => q.Options)
                        .LoadAsync();
                        
                    _logger.LogDebug("Loaded {Count} options for question {QuestionNumber}",
                        question.Options?.Count ?? 0, question.QuestionNumber);
                }
            }
            else
            {
                _logger.LogDebug("No questions found, trying manual query for quiz ID={QuizId}", quiz.Id);
                
                // Fallback: Try to manually query for questions if none were loaded
                List<QuizQuestion> questions = await _context.QuizQuestions
                    .Where(q => q.QuizId == quiz.Id)
                    .ToListAsync();
                
                _logger.LogDebug("Direct query found {Count} questions for quiz ID={QuizId}", 
                    questions.Count, quiz.Id);
                
                if (questions.Count > 0)
                {
                    quiz.Questions = questions;
                    
                    // Load options for each question
                    foreach (QuizQuestion question in quiz.Questions)
                    {
                        List<QuizOption> options = await _context.QuizOptions
                            .Where(o => o.QuestionId == question.Id)
                            .ToListAsync();
                        
                        _logger.LogDebug("Direct query found {Count} options for question {QuestionNumber}",
                            options.Count, question.QuestionNumber);
                        
                        question.Options = options;
                    }
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
