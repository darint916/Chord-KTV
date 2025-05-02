using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ChordKTV.Data.Api.UserData;
using ChordKTV.Data.Api.SongData;
using ChordKTV.Dtos.UserActivity;
using ChordKTV.Models.Quiz;
using ChordKTV.Models.Playlist;
using ChordKTV.Models.SongData;
using ChordKTV.Models.Handwriting;
using AutoMapper;
using ChordKTV.Models.UserData;
using ChordKTV.Dtos;
using ChordKTV.Services.Api;
using ChordKTV.Utils.Extensions;
using ChordKTV.Data.Api.QuizData;
namespace ChordKTV.Controllers;

[ApiController]
[Authorize]
[Route("api/user/activity")]
public class UserActivityController : Controller
{
    private readonly IUserActivityRepo _activityRepo;
    private readonly ILogger<UserActivityController> _logger;
    private readonly IMapper _mapper;
    private readonly IUserContextService _userContextService;
    private readonly ISongRepo _songRepo;
    private readonly IYouTubeClientService _youtubeClientService;
    public UserActivityController(
        IUserActivityRepo activityRepo,
        ILogger<UserActivityController> logger,
        IMapper mapper,
        IUserContextService userContextService,
        ISongRepo songRepo,
        IQuizRepo quizRepo,
        IYouTubeClientService youTubeClientService
        )
    {
        _activityRepo = activityRepo;
        _logger = logger;
        _mapper = mapper;
        _userContextService = userContextService;
        _songRepo = songRepo;
        _youtubeClientService = youTubeClientService;
    }

    [HttpPost("playlist")]
    [ProducesResponseType(typeof(UserPlaylistActivityDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddOrUpdatePlaylistActivity([FromBody] UserPlaylistActivityDto dto)
    {
        try
        {
            User? user = await _userContextService.GetCurrentUserAsync();
            if (user is null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            if (string.IsNullOrWhiteSpace(dto.PlaylistId))
            {
                return BadRequest(new { message = "Invalid playlist Id." });
            }

            UserPlaylistActivity activity = _mapper.Map<UserPlaylistActivity>(dto);
            activity.UserId = user.Id;

            await _activityRepo.UpsertPlaylistActivityAsync(activity);

            return CreatedAtAction(nameof(GetUserPlaylistActivities), new { id = activity.Id }, _mapper.Map<UserPlaylistActivityDto>(activity));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding/updating playlist activity");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpGet("playlists")]
    [ProducesResponseType(typeof(IEnumerable<UserPlaylistActivityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserPlaylistActivities()
    {
        User? user = await _userContextService.GetCurrentUserAsync();
        if (user is null)
        {
            return Unauthorized(new { message = "User not found" });
        }

        IEnumerable<UserPlaylistActivity> activities = await _activityRepo.GetUserPlaylistActivitiesAsync(user.Id);
        return Ok(_mapper.Map<IEnumerable<UserPlaylistActivityDto>>(activities));
    }

    [HttpPost("quiz")]
    [ProducesResponseType(typeof(UserQuizResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddQuizResult([FromBody] UserQuizResultDto dto)
    {
        try
        {
            User? user = await _userContextService.GetCurrentUserAsync();
            if (user is null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            UserQuizResult result = _mapper.Map<UserQuizResult>(dto);
            result.UserId = user.Id;
            result.DateCompleted = dto.DateCompleted ?? DateTime.UtcNow;

            await _activityRepo.ProcessQuizResultAsync(result, dto.CorrectAnswers, dto.Language);

            return CreatedAtAction(nameof(GetUserQuizResults), new { id = result.Id }, _mapper.Map<UserQuizResultDto>(result));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError(ex, "Quiz ID not found or invalid");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding quiz result");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpGet("quizzes")]
    [ProducesResponseType(typeof(IEnumerable<UserQuizResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserQuizResults([FromQuery] LanguageCode? language = null)
    {
        User? user = await _userContextService.GetCurrentUserAsync();
        if (user is null)
        {
            return Unauthorized(new { message = "User not found" });
        }

        IEnumerable<UserQuizResult> quizResults = await _activityRepo.GetUserQuizResultsAsync(user.Id, language);
        return Ok(_mapper.Map<IEnumerable<UserQuizResultDto>>(quizResults));
    }

    [HttpPost("song")]
    [ProducesResponseType(typeof(UserSongActivityDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(UserSongActivityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddOrUpdateSongActivity([FromBody] UserSongActivityDto dto)
    {
        try
        {
            User? user = await _userContextService.GetCurrentUserAsync();
            if (user is null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            Song? song = await _songRepo.GetSongByIdAsync(dto.SongId);
            if (song == null)
            {
                return NotFound(new { message = "Song not found." });
            }

            UserSongActivity activity = _mapper.Map<UserSongActivity>(dto);
            activity.UserId = user.Id;

            await _activityRepo.UpsertSongActivityAsync(activity);

            return CreatedAtAction(nameof(GetUserSongActivities), new { id = activity.Id }, _mapper.Map<UserSongActivityDto>(activity));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding/updating song activity");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpGet("songs")]
    [ProducesResponseType(typeof(IEnumerable<UserSongActivityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserSongActivities()
    {
        User? user = await _userContextService.GetCurrentUserAsync();
        if (user == null)
        { return Unauthorized(); }

        IEnumerable<UserSongActivity> activities = await _activityRepo.GetUserSongActivitiesAsync(user.Id);
        IEnumerable<UserSongActivityDto> dtos = _mapper.Map<IEnumerable<UserSongActivityDto>>(activities);
        return Ok(dtos);
    }

    [HttpPost("handwriting")]
    [ProducesResponseType(typeof(UserHandwritingResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddHandwritingResult([FromBody] UserHandwritingResultDto dto)
    {
        try
        {
            User? user = await _userContextService.GetCurrentUserAsync();
            if (user is null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            UserHandwritingResult result = _mapper.Map<UserHandwritingResult>(dto);
            result.UserId = user.Id;
            result.DateCompleted = dto.DateCompleted ?? DateTime.UtcNow;

            await _activityRepo.AddHandwritingResultAsync(result);

            return CreatedAtAction(nameof(GetUserHandwritingResults), new { id = result.Id }, _mapper.Map<UserHandwritingResultDto>(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding handwriting result");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpGet("handwriting")]
    [ProducesResponseType(typeof(IEnumerable<UserHandwritingResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserHandwritingResults([FromQuery] LanguageCode? language = null)
    {
        User? user = await _userContextService.GetCurrentUserAsync();
        if (user is null)
        {
            return Unauthorized(new { message = "User not found" });
        }

        IEnumerable<UserHandwritingResult> results = await _activityRepo.GetUserHandwritingResultsAsync(user.Id, language);
        return Ok(_mapper.Map<IEnumerable<UserHandwritingResultDto>>(results));
    }

    [HttpPatch("favorite/song")]
    [ProducesResponseType(typeof(UserSongActivityDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(UserSongActivityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ToggleFavoriteSong([FromBody] UserSongActivityFavoriteRequestDto dto)
    {
        try
        {
            User? user = await _userContextService.GetCurrentUserAsync();
            if (user is null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            UserSongActivity? userSongActivity = await _activityRepo.GetUserSongActivityAsync(user.Id, dto.SongId);
            if (userSongActivity is null) //try to add new row
            {
                Song? song = await _songRepo.GetSongByIdAsync(dto.SongId);
                if (song == null)
                {
                    return NotFound(new { message = "Song not found." });
                }
                await _activityRepo.InsertSongActivityAsync(user.Id, dto.IsFavorite, song);
                return CreatedAtAction(nameof(GetUserSongActivities), new { id = dto.SongId }, new { message = dto.IsFavorite ? "Song favorited" : "Song unfavorited" });
            }
            else //update existing fav
            {
                await _activityRepo.UpdateSongActivityFavoriteAsync(userSongActivity, dto.IsFavorite, dto.IsPlayed);
                return Ok(new { message = dto.IsFavorite ? "Song favorited" : "Song unfavorited" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling favorite song");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpGet("favorite/songs")]
    [ProducesResponseType(typeof(IEnumerable<UserSongActivityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFavoriteSongs()
    {
        try
        {
            User? user = await _userContextService.GetCurrentUserAsync();
            if (user is null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            IEnumerable<UserSongActivity> favorites = await _activityRepo.GetFavoriteSongActivitiesAsync(user.Id);
            return Ok(_mapper.Map<IEnumerable<UserSongActivityDto>>(favorites));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving favorite songs");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPatch("favorite/playlist")]
    [ProducesResponseType(typeof(UserPlaylistActivityDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(UserPlaylistActivityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ToggleFavoritePlaylist([FromBody] UserPlaylistActivityFavoriteRequestDto dto)
    {
        try
        {
            User? user = await _userContextService.GetCurrentUserAsync();
            if (user is null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            if (string.IsNullOrWhiteSpace(dto.PlaylistId))
            {
                return BadRequest(new { message = "Invalid playlist URL." });
            }
            UserPlaylistActivity? userPlaylistActivity = await _activityRepo.GetUserPlaylistActivityAsync(user.Id, dto.PlaylistId); //TODO: use playlist id not url
            if (userPlaylistActivity is null)
            {
                PlaylistDetailsDto? playlistDetailsDto = await _youtubeClientService.GetPlaylistDetailsAsync(dto.PlaylistId, false, true);
                if (playlistDetailsDto is null)
                {
                    return NotFound(new { message = "Playlist not found." });
                }
                await _activityRepo.InsertPlaylistActivityAsync(user.Id, dto.IsFavorite, dto.PlaylistId, playlistDetailsDto.PlaylistThumbnailUrl ?? "", playlistDetailsDto.PlaylistTitle);
                return CreatedAtAction(nameof(GetUserPlaylistActivities), new { id = dto.PlaylistId }, new { message = dto.IsFavorite ? "Playlist favorited" : "Playlist unfavorited" });
            }
            else
            { //TODO migrate this to service
                userPlaylistActivity.IsFavorite = dto.IsFavorite;
                userPlaylistActivity.DateFavorited = dto.IsFavorite ? DateTime.UtcNow : null;
                if (dto.IsPlayed)
                {
                    DateTime now = DateTime.UtcNow;
                    userPlaylistActivity.DatesPlayed.Add(now);
                    userPlaylistActivity.LastPlayed = now;
                }
                await _activityRepo.SaveChangesAsync();
                return Ok(new { message = dto.IsFavorite ? "Playlist favorited" : "Playlist unfavorited" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling favorite playlist");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpGet("favorite/playlists")]
    [ProducesResponseType(typeof(IEnumerable<UserPlaylistActivityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFavoritePlaylists()
    {
        try
        {
            User? user = await _userContextService.GetCurrentUserAsync();
            if (user is null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            IEnumerable<UserPlaylistActivity> favorites = await _activityRepo.GetFavoritePlaylistActivitiesAsync(user.Id);
            return Ok(_mapper.Map<IEnumerable<UserPlaylistActivityDto>>(favorites));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving favorite playlists");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPost("learned-word")]
    [ProducesResponseType(typeof(LearnedWordDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddLearnedWord([FromBody] LearnedWordDto dto)
    {
        try
        {
            User? user = await _userContextService.GetCurrentUserAsync();
            if (user is null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            LearnedWord learnedWord = _mapper.Map<LearnedWord>(dto);
            learnedWord.UserId = user.Id;
            learnedWord.DateLearned = dto.DateLearned ?? DateTime.UtcNow;
            await _activityRepo.AddLearnedWordAsync(learnedWord);
            return CreatedAtAction(nameof(GetUserLearnedWords), new { id = learnedWord.Id }, _mapper.Map<LearnedWordDto>(learnedWord));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding learned word");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpGet("learned-words")]
    [ProducesResponseType(typeof(IEnumerable<LearnedWordDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserLearnedWords([FromQuery] LanguageCode? language = null)
    {
        User? user = await _userContextService.GetCurrentUserAsync();
        if (user is null)
        {
            return Unauthorized(new { message = "User not found" });
        }

        IEnumerable<LearnedWord> learnedWords = await _activityRepo.GetUserLearnedWordsAsync(user.Id, language);
        return Ok(_mapper.Map<IEnumerable<LearnedWordDto>>(learnedWords));
    }

    [HttpGet("full")]
    [DevelopmentOnly]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserActivityHistory()
    {
        User? user = await _userContextService.GetCurrentUserAsync();
        if (user is null)
        {
            return Unauthorized(new { message = "User not found" });
        }

        IEnumerable<UserSongActivity> songActivities = await _activityRepo.GetUserSongActivitiesAsync(user.Id);
        IEnumerable<UserPlaylistActivity> playlistActivities = await _activityRepo.GetUserPlaylistActivitiesAsync(user.Id);
        IEnumerable<UserQuizResult> quizResults = await _activityRepo.GetUserQuizResultsAsync(user.Id);
        IEnumerable<UserHandwritingResult> handwritingResults = await _activityRepo.GetUserHandwritingResultsAsync(user.Id);
        IEnumerable<LearnedWord> learnedWords = await _activityRepo.GetUserLearnedWordsAsync(user.Id);

        object response = new
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
}
