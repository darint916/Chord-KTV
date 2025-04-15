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

namespace ChordKTV.Controllers;

[ApiController]
[Authorize]
[Route("api/user/activity")]
public class UserActivityController : Controller
{
    private readonly IUserActivityRepo _activityRepo;
    private readonly IUserRepo _userRepo;
    private readonly ILogger<UserActivityController> _logger;

    public UserActivityController(
        IUserActivityRepo activityRepo,
        IUserRepo userRepo,
        ILogger<UserActivityController> logger)
    {
        _activityRepo = activityRepo;
        _userRepo = userRepo;
        _logger = logger;
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

            UserPlaylistActivity? existingActivity = await _activityRepo.GetPlaylistActivityAsync(user.Id, dto.PlaylistUrl);

            if (existingActivity != null)
            {
                existingActivity.PlayCount++;
                existingActivity.LastPlayed = dto.DatePlayed ?? DateTime.UtcNow;
            }
            else
            {
                existingActivity = new UserPlaylistActivity
                {
                    UserId = user.Id,
                    PlaylistUrl = dto.PlaylistUrl,
                    PlayCount = 1,
                    LastPlayed = dto.DatePlayed ?? DateTime.UtcNow
                };
                await _activityRepo.AddPlaylistActivityAsync(existingActivity);
            }

            await _activityRepo.SaveChangesAsync();
            return Ok(existingActivity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding playlist activity");
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
        return Ok(activities.Select(pa => new
        {
            pa.Id,
            pa.PlaylistUrl,
            pa.PlayCount,
            pa.LastPlayed
        }));
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

            var result = new UserQuizResult
            {
                UserId = user.Id,
                QuizId = dto.QuizId,
                Score = dto.Score,
                Language = dto.Language,
                DateCompleted = dto.DateCompleted ?? DateTime.UtcNow
            };

            await _activityRepo.AddQuizResultAsync(result);

            // Get existing learned words for this user and language
            IEnumerable<LearnedWord> existingWords = await _activityRepo.GetUserLearnedWordsAsync(user.Id, dto.Language);
            var existingWordSet = existingWords.Select(w => w.Word).ToHashSet();

            // Add only new learned words from correct answers
            foreach (string word in dto.CorrectAnswers.Where(w => !existingWordSet.Contains(w)))
            {
                await _activityRepo.AddLearnedWordAsync(new LearnedWord
                {
                    UserId = user.Id,
                    Word = word,
                    Language = dto.Language,
                    DateLearned = DateTime.UtcNow
                });
            }

            await _activityRepo.SaveChangesAsync();

            return Ok(new
            {
                result.Id,
                result.QuizId,
                result.Score,
                result.Language,
                result.DateCompleted,
                result.UserId,
                dto.CorrectAnswers
            });
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

    [HttpPost("songplay")]
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
                DatePlayed = dto.DatePlayed ?? DateTime.UtcNow
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

    [HttpGet("songplays")]
    public async Task<IActionResult> GetUserSongPlays()
    {
        User? user = await GetUserFromClaimsAsync();
        if (user is null)
        {
            return Unauthorized(new { message = "User not found" });
        }

        IEnumerable<UserSongPlay> songPlays = await _activityRepo.GetUserSongPlaysAsync(user.Id);
        return Ok(songPlays.Select(sp => new
        {
            sp.Id,
            sp.SongId,
            sp.DatePlayed
        }));
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

            UserFavoriteSong? existingFavorite = await _activityRepo.GetFavoriteSongAsync(user.Id, dto.SongId);

            if (dto.Favorited)
            {
                if (existingFavorite == null)
                {
                    var favorite = new UserFavoriteSong
                    {
                        UserId = user.Id,
                        SongId = dto.SongId,
                        DateFavorited = DateTime.UtcNow
                    };
                    await _activityRepo.AddFavoriteSongAsync(favorite);
                }
                else
                {
                    existingFavorite.DateFavorited = DateTime.UtcNow;
                }
            }
            else if (existingFavorite != null)
            {
                await _activityRepo.RemoveFavoriteSongAsync(existingFavorite);
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

            IEnumerable<UserFavoriteSong> favorites = await _activityRepo.GetUserFavoriteSongsAsync(user.Id);
            return Ok(favorites.Select(f => new
            {
                f.Id,
                f.SongId,
                f.DateFavorited
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting favorite songs");
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

            UserFavoritePlaylist? existingFavorite = await _activityRepo.GetFavoritePlaylistAsync(user.Id, dto.PlaylistUrl);

            if (dto.Favorited)
            {
                if (existingFavorite == null)
                {
                    var favorite = new UserFavoritePlaylist
                    {
                        UserId = user.Id,
                        PlaylistUrl = dto.PlaylistUrl,
                        DateFavorited = DateTime.UtcNow
                    };
                    await _activityRepo.AddFavoritePlaylistAsync(favorite);
                }
                else
                {
                    existingFavorite.DateFavorited = DateTime.UtcNow;
                }
            }
            else if (existingFavorite != null)
            {
                await _activityRepo.RemoveFavoritePlaylistAsync(existingFavorite);
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

            IEnumerable<UserFavoritePlaylist> favorites = await _activityRepo.GetUserFavoritePlaylistsAsync(user.Id);
            return Ok(favorites.Select(f => new
            {
                f.Id,
                f.PlaylistUrl,
                f.DateFavorited
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting favorite playlists");
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

            var learnedWord = new LearnedWord
            {
                UserId = user.Id,
                Word = dto.Word,
                Language = dto.Language,
                DateLearned = dto.DateLearned ?? DateTime.UtcNow
            };

            await _activityRepo.AddLearnedWordAsync(learnedWord);
            await _activityRepo.SaveChangesAsync();

            return Ok(new
            {
                learnedWord.Id,
                learnedWord.Word,
                learnedWord.Language,
                learnedWord.DateLearned
            });
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
        return Ok(learnedWords.Select(lw => new
        {
            lw.Id,
            lw.Word,
            lw.Language,
            lw.DateLearned
        }));
    }

    [HttpGet("full")]
    public async Task<IActionResult> GetUserActivityHistory()
    {
        User? user = await GetUserFromClaimsAsync();
        if (user is null)
        {
            return Unauthorized(new { message = "User not found" });
        }

        var activities = new
        {
            PlaylistActivities = (await _activityRepo.GetUserPlaylistActivitiesAsync(user.Id))
                .Select(pa => new
                {
                    pa.Id,
                    pa.PlaylistUrl,
                    pa.PlayCount,
                    pa.LastPlayed
                }),
            QuizzesDone = (await _activityRepo.GetUserQuizResultsAsync(user.Id))
                .Select(qr => new
                {
                    qr.Id,
                    qr.QuizId,
                    qr.Score,
                    qr.Language,
                    qr.DateCompleted
                }),
            SongPlays = (await _activityRepo.GetUserSongPlaysAsync(user.Id))
                .Select(sp => new
                {
                    sp.Id,
                    sp.SongId,
                    sp.DatePlayed
                }),
            FavoriteSongs = (await _activityRepo.GetUserFavoriteSongsAsync(user.Id))
                .Select(fs => new
                {
                    fs.Id,
                    fs.SongId,
                    fs.DateFavorited
                }),
            FavoritePlaylists = (await _activityRepo.GetUserFavoritePlaylistsAsync(user.Id))
                .Select(fp => new
                {
                    fp.Id,
                    fp.PlaylistUrl,
                    fp.DateFavorited
                }),
            HandwritingResults = (await _activityRepo.GetUserHandwritingResultsAsync(user.Id))
                .Select(hr => new
                {
                    hr.Id,
                    hr.Language,
                    hr.Score,
                    hr.WordTested,
                    hr.DateCompleted
                }),
            LearnedWords = (await _activityRepo.GetUserLearnedWordsAsync(user.Id))
                .Select(lw => new
                {
                    lw.Id,
                    lw.Word,
                    lw.Language,
                    lw.DateLearned
                }),
            Message = "Activity history retrieved successfully. If lists are empty, you haven't recorded any activity yet."
        };

        return Ok(activities);
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
