using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ChordKTV.Data.Api.UserData;
using ChordKTV.Dtos.UserActivity;
using ChordKTV.Models.Quiz;
using ChordKTV.Models.Playlist;
using ChordKTV.Models.SongData;
using ChordKTV.Models.Handwriting;
using ChordKTV.Services.Api;
using AutoMapper;
using ChordKTV.Models.UserData;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ChordKTV.Controllers;

[ApiController]
[Authorize]
[Route("api/user/activity")]
public class UserActivityController : Controller
{
    private readonly IUserActivityRepo _activityRepo;
    private readonly IUserRepo _userRepo;
    private readonly ILogger<UserActivityController> _logger;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly IUserService _userService;

    public UserActivityController(
        IUserActivityRepo activityRepo,
        IUserRepo userRepo,
        IMapper mapper,
        ILogger<UserActivityController> logger,
        IConfiguration configuration,
        IUserService userService)
    {
        _activityRepo = activityRepo;
        _userRepo = userRepo;
        _mapper = mapper;
        _logger = logger;
        _configuration = configuration;
        _userService = userService;
    }

    [HttpPost("playlist")]
    public async Task<IActionResult> AddPlaylistActivity([FromBody] UserPlaylistActivityDto dto)
    {
        try
        {
            User? user = await GetUserFromClaimsAsync();
            if (user is null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            UserPlaylistActivity? activity = await _activityRepo.GetPlaylistActivityAsync(user.Id, dto.PlaylistUrl) 
                ?? new UserPlaylistActivity
                {
                    UserId = user.Id,
                    PlaylistUrl = dto.PlaylistUrl,
                    PlayCount = 0,
                    LastPlayed = DateTime.UtcNow
                };

            activity.PlayCount++;
            activity.LastPlayed = dto.PlayedAt;

            if (activity.Id == Guid.Empty)
            {
                await _activityRepo.AddPlaylistActivityAsync(activity);
            }

            await _activityRepo.SaveChangesAsync();
            return Ok(activity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding playlist activity");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPost("quiz")]
    public async Task<IActionResult> AddQuizResult([FromBody] UserQuizResultDto dto)
    {
        try
        {
            User? user = await GetUserFromClaimsAsync();
            if (user is null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            UserQuizResult result = new UserQuizResult
            {
                UserId = user.Id,
                QuizId = dto.QuizId,
                Score = dto.Score,
                Language = dto.Language,
                CompletedAt = dto.CompletedAt
            };

            await _activityRepo.AddQuizResultAsync(result);

            // Add learned words for correct answers
            foreach (string word in dto.CorrectAnswers)
            {
                await _activityRepo.AddLearnedWordAsync(new LearnedWord
                {
                    UserId = user.Id,
                    Word = word,
                    Language = dto.Language,
                    LearnedOn = dto.CompletedAt
                });
            }

            await _activityRepo.SaveChangesAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding quiz result");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPost("song")]
    public async Task<IActionResult> AddSongPlay([FromBody] UserSongPlayDto dto)
    {
        try
        {
            User? user = await GetUserFromClaimsAsync();
            if (user is null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            UserSongPlay songPlay = new UserSongPlay
            {
                UserId = user.Id,
                SongId = dto.SongId,
                PlayedAt = dto.PlayedAt
            };

            await _activityRepo.AddSongPlayAsync(songPlay);
            await _activityRepo.SaveChangesAsync();
            return Ok(songPlay);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding song play");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPost("handwriting")]
    public async Task<IActionResult> AddHandwritingResult([FromBody] UserHandwritingResultDto dto)
    {
        try
        {
            User? user = await GetUserFromClaimsAsync();
            if (user is null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            UserHandwritingResult result = new UserHandwritingResult
            {
                UserId = user.Id,
                Language = dto.Language,
                Score = dto.Score,
                WordsTested = dto.WordsTested,
                CompletedAt = dto.CompletedAt
            };

            await _activityRepo.AddHandwritingResultAsync(result);

            // Add words as learned if score is good (above 80%)
            if (dto.Score >= 80)
            {
                foreach (string word in dto.WordsTested)
                {
                    await _activityRepo.AddLearnedWordAsync(new LearnedWord
                    {
                        UserId = user.Id,
                        Word = word,
                        Language = dto.Language,
                        LearnedOn = dto.CompletedAt
                    });
                }
            }

            await _activityRepo.SaveChangesAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding handwriting result");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetUserActivityHistory()
    {
        _logger.LogInformation("=== Starting GetUserActivityHistory ===");
        _logger.LogInformation("Request headers:");
        foreach (var header in Request.Headers)
        {
            _logger.LogInformation("{Key}: {Value}", header.Key, header.Value);
        }
        
        _logger.LogInformation("Auth type: {AuthType}", User?.Identity?.AuthenticationType ?? "none");
        _logger.LogInformation("Is authenticated: {IsAuthenticated}", User?.Identity?.IsAuthenticated ?? false);
        
        if (User?.Claims != null)
        {
            _logger.LogInformation("Claims present:");
            foreach (var claim in User.Claims)
            {
                _logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
            }
        }
        else
        {
            _logger.LogWarning("No claims present in User object");
        }

        try
        {
            User? user = await GetUserFromClaimsAsync();
            if (user is null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            // Get the data (which might be empty)
            var quizResults = await _activityRepo.GetUserQuizResultsAsync(user.Id);
            var songPlays = await _activityRepo.GetUserSongPlaysAsync(user.Id);
            var handwritingResults = await _activityRepo.GetUserHandwritingResultsAsync(user.Id);
            var learnedWords = await _activityRepo.GetUserLearnedWordsAsync(user.Id);

            // Return empty data with a 200 OK status
            object activities = new
            {
                QuizResults = quizResults,
                SongPlays = songPlays,
                HandwritingResults = handwritingResults,
                LearnedWords = learnedWords,
                Message = "Activity history retrieved successfully. If lists are empty, you haven't recorded any activity yet."
            };

            return Ok(activities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user activity history");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    private async Task<User?> GetUserFromClaimsAsync()
    {
        _logger.LogInformation("Claims present: {Claims}", 
            string.Join(", ", User.Claims.Select(c => $"{c.Type}: {c.Value}")));
        
        var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("No email claim found in token");
            return null;
        }
        
        var user = await _userRepo.GetUserByEmailAsync(email);
        if (user == null)
        {
            _logger.LogWarning("User not found for email: {Email}", email);
        }
        
        return user;
    }
} 