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
using ChordKTV.Dtos;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

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

    public UserActivityController(
        IUserActivityRepo activityRepo,
        IUserRepo userRepo,
        ILogger<UserActivityController> logger,
        IMapper mapper)
    {
        _activityRepo = activityRepo;
        _userRepo = userRepo;
        _logger = logger;
        _mapper = mapper;
    }

    [HttpPost("playlist")]
    public async Task<IActionResult> AddOrUpdatePlaylistActivity([FromBody] UserPlaylistActivityDto dto)
    {
        try
        {
            User? user = await GetUserFromClaimsAsync();
            if (user is null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            // Ensure at least one play date is provided
            if (dto.DatesPlayed == null || !dto.DatesPlayed.Any())
            {
                dto.DatesPlayed.Add(DateTime.UtcNow);
            }

            UserPlaylistActivity? existing = await _activityRepo.GetUserPlaylistActivityAsync(user.Id, dto.PlaylistUrl);
            if (existing == null)
            {
                var activity = _mapper.Map<UserPlaylistActivity>(dto);
                activity.UserId = user.Id;
                await _activityRepo.UpsertPlaylistActivityAsync(activity);
                await _activityRepo.SaveChangesAsync();
                var resultDto = _mapper.Map<UserPlaylistActivityDto>(activity);
                return Ok(resultDto);
            }
            else
            {
                // Merge new play dates and update favorite status
                foreach (var date in dto.DatesPlayed)
                {
                    if (!existing.DatesPlayed.Contains(date))
                    {
                        existing.DatesPlayed.Add(date);
                    }
                }
                existing.IsFavorite = dto.IsFavorite;
                await _activityRepo.UpsertPlaylistActivityAsync(existing);
                await _activityRepo.SaveChangesAsync();
                var resultDto = _mapper.Map<UserPlaylistActivityDto>(existing);
                return Ok(resultDto);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding/updating playlist activity");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpGet("playlists")]
    public async Task<IActionResult> GetUserPlaylistActivities()
    {
        User? user = await GetUserFromClaimsAsync();
        if (user is null)
        {
            return Unauthorized(new { message = "User not found" });
        }

        IEnumerable<UserPlaylistActivity> activities = await _activityRepo.GetUserPlaylistActivitiesAsync(user.Id);
        var result = _mapper.Map<IEnumerable<UserPlaylistActivityDto>>(activities);
        return Ok(result);
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

            var result = _mapper.Map<UserQuizResult>(dto);
            result.UserId = user.Id;
            // Ensure DateCompleted is set
            result.DateCompleted = dto.DateCompleted ?? DateTime.UtcNow;

            await _activityRepo.AddQuizResultAsync(result);

            // Optionally add learned words for any new correct answers
            IEnumerable<LearnedWord> learnedWords = await _activityRepo.GetUserLearnedWordsAsync(user.Id, dto.Language);
            var existingWords = new HashSet<string>(learnedWords.Select(lw => lw.Word));
            foreach (string word in dto.CorrectAnswers.Where(w => !existingWords.Contains(w)))
            {
                var lw = new LearnedWord
                {
                    UserId = user.Id,
                    Word = word,
                    Language = dto.Language,
                    DateLearned = DateTime.UtcNow
                };
                await _activityRepo.AddLearnedWordAsync(lw);
            }

            await _activityRepo.SaveChangesAsync();
            var resultDto = _mapper.Map<UserQuizResultDto>(result);
            return Ok(resultDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding quiz result");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpGet("quizzes")]
    public async Task<IActionResult> GetUserQuizResults()
    {
        User? user = await GetUserFromClaimsAsync();
        if (user is null)
        {
            return Unauthorized(new { message = "User not found" });
        }

        IEnumerable<UserQuizResult> quizResults = await _activityRepo.GetUserQuizResultsAsync(user.Id);
        return Ok(quizResults.Select(qr => new
        {
            qr.Id,
            qr.QuizId,
            qr.Score,
            qr.Language,
            qr.DateCompleted
        }));
    }

    [HttpPost("song")]
    public async Task<IActionResult> AddOrUpdateSongActivity([FromBody] UserSongActivityDto dto)
    {
        try
        {
            User? user = await GetUserFromClaimsAsync();
            if (user is null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            // Ensure at least one play date is provided
            if (dto.DatesPlayed == null || !dto.DatesPlayed.Any())
            {
                dto.DatesPlayed.Add(DateTime.UtcNow);
            }

            UserSongActivity? existing = await _activityRepo.GetUserSongActivityAsync(user.Id, dto.SongId);
            if (existing == null)
            {
                var activity = _mapper.Map<UserSongActivity>(dto);
                activity.UserId = user.Id;
                await _activityRepo.UpsertSongActivityAsync(activity);
                await _activityRepo.SaveChangesAsync();
                var resultDto = _mapper.Map<UserSongActivityDto>(activity);
                return Ok(resultDto);
            }
            else
            {
                // Merge any new play dates (avoiding duplicates) and update favorite status
                foreach (var date in dto.DatesPlayed)
                {
                    if (!existing.DatesPlayed.Contains(date))
                    {
                        existing.DatesPlayed.Add(date);
                    }
                }
                existing.IsFavorite = dto.IsFavorite;
                await _activityRepo.UpsertSongActivityAsync(existing);
                await _activityRepo.SaveChangesAsync();
                var resultDto = _mapper.Map<UserSongActivityDto>(existing);
                return Ok(resultDto);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding/updating song activity");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpGet("songs")]
    public async Task<IActionResult> GetUserSongActivities()
    {
        try
        {
            User? user = await GetUserFromClaimsAsync();
            if (user is null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            IEnumerable<UserSongActivity> songActivities = await _activityRepo.GetUserSongActivitiesAsync(user.Id);
            var result = _mapper.Map<IEnumerable<UserSongActivityDto>>(songActivities);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving song activities");
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

            var result = new UserHandwritingResult
            {
                UserId = user.Id,
                Language = dto.Language,
                Score = dto.Score,
                WordTested = dto.WordTested,
                DateCompleted = dto.DateCompleted ?? DateTime.UtcNow
            };

            await _activityRepo.AddHandwritingResultAsync(result);
            await _activityRepo.SaveChangesAsync();

            return Ok(new
            {
                result.Id,
                result.Language,
                result.Score,
                result.WordTested,
                result.DateCompleted,
                result.UserId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding handwriting result");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpGet("handwriting")]
    public async Task<IActionResult> GetUserHandwritingResults()
    {
        User? user = await GetUserFromClaimsAsync();
        if (user is null)
        {
            return Unauthorized(new { message = "User not found" });
        }

        IEnumerable<UserHandwritingResult> results = await _activityRepo.GetUserHandwritingResultsAsync(user.Id);
        return Ok(results.Select(hr => new
        {
            hr.Id,
            hr.Language,
            hr.Score,
            hr.WordTested,
            hr.DateCompleted
        }));
    }

    [HttpPost("favorite/song")]
    public async Task<IActionResult> ToggleFavoriteSong([FromBody] FavoriteSongDto dto)
    {
        try
        {
            User? user = await GetUserFromClaimsAsync();
            if (user is null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            UserSongActivity? activity = await _activityRepo.GetUserSongActivityAsync(user.Id, dto.SongId);
            if (activity == null)
            {
                // Create a new activity record with a current timestamp if none exists
                activity = new UserSongActivity
                {
                    UserId = user.Id,
                    SongId = dto.SongId,
                    DatesPlayed = new List<DateTime> { DateTime.UtcNow },
                    IsFavorite = dto.Favorited
                };
                await _activityRepo.UpsertSongActivityAsync(activity);
            }
            else
            {
                // Update the favorite flag
                activity.IsFavorite = dto.Favorited;
                if (!activity.DatesPlayed.Any())
                {
                    activity.DatesPlayed.Add(DateTime.UtcNow);
                }
                await _activityRepo.UpsertSongActivityAsync(activity);
            }
            await _activityRepo.SaveChangesAsync();
            return Ok(new { message = dto.Favorited ? "Song favorited" : "Song unfavorited" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling favorite song");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpGet("favorite/songs")]
    public async Task<IActionResult> GetFavoriteSongs()
    {
        try
        {
            User? user = await GetUserFromClaimsAsync();
            if (user is null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            IEnumerable<UserSongActivity> allSongs = await _activityRepo.GetUserSongActivitiesAsync(user.Id);
            var favorites = allSongs.Where(sa => sa.IsFavorite).ToList();
            var result = _mapper.Map<IEnumerable<UserSongActivityDto>>(favorites);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving favorite songs");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPost("favorite/playlist")]
    public async Task<IActionResult> ToggleFavoritePlaylist([FromBody] FavoritePlaylistDto dto)
    {
        try
        {
            User? user = await GetUserFromClaimsAsync();
            if (user is null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            UserPlaylistActivity? activity = await _activityRepo.GetUserPlaylistActivityAsync(user.Id, dto.PlaylistUrl);
            if (activity == null)
            {
                // Create a new activity record with a current timestamp if none exists
                activity = new UserPlaylistActivity
                {
                    UserId = user.Id,
                    PlaylistUrl = dto.PlaylistUrl,
                    DatesPlayed = new List<DateTime> { DateTime.UtcNow },
                    IsFavorite = dto.Favorited
                };
                await _activityRepo.UpsertPlaylistActivityAsync(activity);
            }
            else
            {
                // Update the favorite flag
                activity.IsFavorite = dto.Favorited;
                if (!activity.DatesPlayed.Any())
                {
                    activity.DatesPlayed.Add(DateTime.UtcNow);
                }
                await _activityRepo.UpsertPlaylistActivityAsync(activity);
            }
            await _activityRepo.SaveChangesAsync();
            return Ok(new { message = dto.Favorited ? "Playlist favorited" : "Playlist unfavorited" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling favorite playlist");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpGet("favorite/playlists")]
    public async Task<IActionResult> GetFavoritePlaylists()
    {
        try
        {
            User? user = await GetUserFromClaimsAsync();
            if (user is null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            IEnumerable<UserPlaylistActivity> allPlaylists = await _activityRepo.GetUserPlaylistActivitiesAsync(user.Id);
            var favorites = allPlaylists.Where(pa => pa.IsFavorite).ToList();
            var result = _mapper.Map<IEnumerable<UserPlaylistActivityDto>>(favorites);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving favorite playlists");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPost("learned-word")]
    public async Task<IActionResult> AddLearnedWord([FromBody] LearnedWordDto dto)
    {
        try
        {
            User? user = await GetUserFromClaimsAsync();
            if (user is null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            var learnedWord = _mapper.Map<LearnedWord>(dto);
            learnedWord.UserId = user.Id;
            // Ensure DateLearned is set
            learnedWord.DateLearned = dto.DateLearned ?? DateTime.UtcNow;
            await _activityRepo.AddLearnedWordAsync(learnedWord);
            await _activityRepo.SaveChangesAsync();
            // Optionally, map back to a DTO if one exists.
            var result = _mapper.Map<LearnedWordDto>(learnedWord);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding learned word");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpGet("learned-words")]
    public async Task<IActionResult> GetUserLearnedWords([FromQuery] LanguageCode? language = null)
    {
        User? user = await GetUserFromClaimsAsync();
        if (user is null)
        {
            return Unauthorized(new { message = "User not found" });
        }

        IEnumerable<LearnedWord> learnedWords = await _activityRepo.GetUserLearnedWordsAsync(user.Id, language);
        var result = _mapper.Map<IEnumerable<LearnedWordDto>>(learnedWords);
        return Ok(result);
    }

    [HttpGet("full")]
    public async Task<IActionResult> GetUserActivityHistory()
    {
        User? user = await GetUserFromClaimsAsync();
        if (user is null)
        {
            return Unauthorized(new { message = "User not found" });
        }

        var songActivities = await _activityRepo.GetUserSongActivitiesAsync(user.Id);
        var playlistActivities = await _activityRepo.GetUserPlaylistActivitiesAsync(user.Id);
        var quizResults = await _activityRepo.GetUserQuizResultsAsync(user.Id);
        var handwritingResults = await _activityRepo.GetUserHandwritingResultsAsync(user.Id);
        var learnedWords = await _activityRepo.GetUserLearnedWordsAsync(user.Id);

        var response = new
        {
            SongActivities = _mapper.Map<IEnumerable<UserSongActivityDto>>(songActivities),
            PlaylistActivities = _mapper.Map<IEnumerable<UserPlaylistActivityDto>>(playlistActivities),
            QuizResults = _mapper.Map<IEnumerable<UserQuizResultDto>>(quizResults),
            HandwritingResults = _mapper.Map<IEnumerable<UserHandwritingResultDto>>(handwritingResults),
            LearnedWords = _mapper.Map<IEnumerable<LearnedWordDto>>(learnedWords),
            Message = "Activity history retrieved successfully. If lists are empty, you haven't recorded any activity yet."
        };

        return Ok(response);
    }

    private async Task<User?> GetUserFromClaimsAsync()
    {
        _logger.LogInformation("Claims present: {Claims}",
            string.Join(", ", User.Claims.Select(c => $"{c.Type}: {c.Value}")));

        string? email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("No email claim found in token");
            return null;
        }

        User? user = await _userRepo.GetUserByEmailAsync(email);
        if (user == null)
        {
            _logger.LogWarning("User not found for email: {Email}", email);
        }

        return user;
    }
}
