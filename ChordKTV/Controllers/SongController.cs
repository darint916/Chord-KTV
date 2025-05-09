using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ChordKTV.Services.Api;
using ChordKTV.Dtos;
using ChordKTV.Data.Api.SongData;
using ChordKTV.Models.SongData;
using ChordKTV.Utils.Extensions;
using System.Text.Json;
using AutoMapper;
using ChordKTV.Dtos.FullSong;
using ChordKTV.Dtos.TranslationGptApi;
using ChordKTV.Data.Api.UserData;
using ChordKTV.Models.UserData;
using ChordKTV.Dtos.GeniusApi;
using ChordKTV.Dtos.LrcLib;

namespace ChordKTV.Controllers;

[ApiController]
[Route("api")]
public class SongController : Controller
{
    private readonly ISongRepo _songRepo;
    private readonly IYouTubeClientService _youTubeClientService;
    private readonly ILrcService _lrcService;
    private readonly IGeniusService _geniusService;
    private readonly IAlbumRepo _albumRepo;
    private readonly ILogger<SongController> _logger;
    private readonly IMapper _mapper;
    private readonly IChatGptService _chatGptService;
    private readonly IFullSongService _fullSongService;
    private readonly IUserRepo _userRepo;

    public SongController(
        IMapper mapper,
        IYouTubeClientService youTubeClientService,
        ILrcService lrcService,
        ISongRepo songRepo,
        IGeniusService geniusService,
        IAlbumRepo albumRepo,
        IChatGptService chatGptService,
        IFullSongService fullSongService,
        IUserRepo userRepo,
        ILogger<SongController> logger
        )
    {
        _mapper = mapper;
        _songRepo = songRepo;
        _youTubeClientService = youTubeClientService;
        _lrcService = lrcService;
        _geniusService = geniusService;
        _albumRepo = albumRepo;
        _chatGptService = chatGptService;
        _fullSongService = fullSongService;
        _userRepo = userRepo;
        _logger = logger;
    }

    [HttpGet("youtube/playlists/{playlistId}")]
    [ProducesResponseType(typeof(PlaylistDetailsDto), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetYouTubePlaylist(string playlistId, [FromQuery] bool shuffle = false)
    {
        PlaylistDetailsDto? result = await _youTubeClientService.GetPlaylistDetailsAsync(playlistId, shuffle);
        if (result == null)
        {
            return StatusCode(500, new { message = "Server error: YouTube API key is missing." });
        }

        return Ok(result);
    }

    [HttpGet("lyrics/lrc/match")]
    [ProducesResponseType(typeof(LrcLyricsDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    [ProducesResponseType(503)]
    public async Task<IActionResult> GetLrcLibLyrics([FromQuery] string? title,
                                    [FromQuery] string? artist, [FromQuery] string? albumName,
                                    [FromQuery] float? duration)
    {
        try
        {
            LrcLyricsDto? lyrics = await _lrcService.GetAllLrcLibLyricsAsync(title, artist, albumName, duration);
            if (lyrics == null)
            {
                return NotFound(new { message = "Lyrics not found for the specified track." });
            }

            return Ok(lyrics);
        }
        catch (HttpRequestException ex) // Handles HTTP request errors
        {
            return StatusCode(503, new { message = "HTTP FAIL Failed to fetch lyrics. Service may be unavailable.", error = ex.Message });
        }
        catch (ArgumentException ex) // Handles invalid arguments
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex) // Handles unexpected errors
        {
            return StatusCode(500, new { message = "An unexpected error occurred.", error = ex.Message });
        }
    }

    [HttpPost("lyrics/lrc/translation")]
    [ProducesResponseType(typeof(LrcLyricsDto), 200)]
    public async Task<IActionResult> PostChatGptTranslations([FromBody] TranslationRequestDto request)
    {
        TranslationResponseDto lyricsDto = await _chatGptService.TranslateLyricsAsync(request.OriginalLyrics, request.LanguageCode, request.Romanize, request.Translate);
        return Ok(lyricsDto);
    }

    [HttpPost("songs/match")]
    [ProducesResponseType(typeof(FullSongResponseDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    [ProducesResponseType(503)]
    public async Task<IActionResult> SearchLyrics([FromBody] FullSongRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) && string.IsNullOrWhiteSpace(request.Lyrics) && string.IsNullOrWhiteSpace(request.YouTubeId))
        {
            return BadRequest(new { message = "songs/match: At least one of the following fields is required: title, lyrics." });
        }
        string? lyricsQuery = null;
        if (!string.IsNullOrWhiteSpace(request.Lyrics))
        {
            lyricsQuery = request.Lyrics + " " + request.Title ?? "" + " " + request.Artist ?? "";
        }
        try
        {
            FullSongResponseDto? fullSong = await _fullSongService.GetFullSongAsync(request.Title, request.Artist, request.Album, request.Duration, lyricsQuery, request.YouTubeId);
            if (fullSong == null)
            {
                return NotFound(new { message = "Song search: song not found. null return" });
            }
            return Ok(fullSong);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(503, new { message = "HttpRequestException in songs/match ", error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred.", error = ex.Message });
        }
    }


    [HttpGet("songs/genius/match")]
    [ProducesResponseType(typeof(SongDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    [ProducesResponseType(503)]
    public async Task<IActionResult> GetSongByArtistTitle(
        [FromQuery] string? title,
        [FromQuery] string? artist,
        [FromQuery] string? lyrics,
        [FromQuery] bool forceRefresh = false)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(lyrics))
            {
                return BadRequest(new { message = "At least one of the following is required: title, lyrics" });
            }

            Song? song = await _geniusService.GetSongByArtistTitleAsync(title, artist, lyrics, forceRefresh);
            if (song == null)
            {
                return NotFound(new { message = "Song not found on Genius." });
            }

            return Ok(_mapper.Map<SongDto>(song));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch from Genius API for title: {Title}, artist: {Artist}, lyrics: {Lyrics}",
                title, artist, lyrics);
            return StatusCode(503, new { message = "Failed to fetch from Genius API.", error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching song. Title: {Title}, Artist: {Artist}, Lyrics: {Lyrics}",
                title, artist, lyrics);
            return StatusCode(500, new { message = "An unexpected error occurred.", error = ex.Message });
        }
    }

    [HttpPost("songs/genius/match/batch")]
    [ProducesResponseType(typeof(List<SongDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    [ProducesResponseType(503)]
    public async Task<IActionResult> GetSongsByArtistTitle(
        [FromBody] JsonElement request,
        [FromQuery] bool forceRefresh = false)
    {
        try
        {
            if (!request.TryGetProperty("videos", out JsonElement videosElement))
            {
                return BadRequest(new { message = "The videos field is required." });
            }

            var videos = videosElement.EnumerateArray()
                .Select(v => new VideoInfo(
                    v.GetProperty("title").GetString() ?? string.Empty,
                    v.GetProperty("artist").GetString() ?? string.Empty,
                    string.Empty,
                    TimeSpan.Zero
                ))
                .ToList();

            List<Song> songs = await _geniusService.GetSongsByArtistTitleAsync(videos, forceRefresh);
            if (songs.Count <= 0)
            {
                return NotFound(new { message = "No songs found on Genius." });
            }

            return Ok(_mapper.Map<List<SongDto>>(songs));
        }
        catch (JsonException ex)
        {
            return BadRequest(new { message = "Invalid JSON format.", error = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(503, new { message = "Failed to fetch from Genius API.", error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred.", error = ex.Message });
        }
    }

    [HttpGet("album/{albumName}")]
    [ProducesResponseType(typeof(List<SongDto>), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetSongsByAlbum(
        string albumName,
        [FromQuery] string? artist)
    {
        try
        {
            Album? album;
            if (artist != null)
            {
                album = await _albumRepo.GetAlbumAsync(albumName, artist);
            }
            else
            {
                album = await _albumRepo.GetAlbumAsync(albumName);  // Use the overload that only takes name
            }

            if (album == null)
            {
                return NotFound(new { message = "Album not found." });
            }

            return Ok(_mapper.Map<List<SongDto>>(album.Songs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching songs for album {AlbumName}", albumName);
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpGet("songs/genius/search")]
    [ProducesResponseType(typeof(List<GeniusHit>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    [ProducesResponseType(503)]
    public async Task<IActionResult> GetGeniusSearchResults([FromQuery, Required, MaxLength(200)] string searchQuery)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                return BadRequest(new { message = "Search query is required." });
            }

            List<GeniusHit>? hits = await _geniusService.GetGeniusSearchResultsAsync(searchQuery);
            if (hits == null || hits.Count == 0)
            {
                return NotFound(new { message = "No results found." });
            }
            return Ok(hits);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch from Genius API for search query: {SearchQuery}", searchQuery);
            return StatusCode(503, new { message = "Failed to fetch from Genius API.", error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while searching Genius API. Search query: {SearchQuery}", searchQuery);
            return StatusCode(500, new { message = "An unexpected error occurred.", error = ex.Message });
        }
    }

    [HttpGet("songs/lrclib/search")]
    [ProducesResponseType(typeof(List<LrcLyricsDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    [ProducesResponseType(503)]
    public async Task<IActionResult> GetLrcLibSearchResults([FromQuery, Required, MaxLength(200)] string searchQuery)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                return BadRequest(new { message = "Search query is required." });
            }

            List<LrcLyricsDto>? hits = await _lrcService.GetLrcLibSearchResultsAsync(searchQuery);
            if (hits == null || hits.Count == 0)
            {
                return NotFound(new { message = "No results found." });
            }
            return Ok(_mapper.Map<List<LrcMetaDataDto>>(hits));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch from LRC API for search query: {SearchQuery}", searchQuery);
            return StatusCode(503, new { message = "Failed to fetch from LRC API.", error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while searching LRC API. Search query: {SearchQuery}", searchQuery);
            return StatusCode(500, new { message = "An unexpected error occurred.", error = ex.Message });
        }
    }

    [HttpPut("songs/{songId}/video/instrumental")]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> PutSongVideoInstrumental(string songId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(songId))
            {
                return BadRequest(new { message = "Song ID is required." });
            }
            if (!Guid.TryParse(songId, out Guid songGuid))
            {
                return BadRequest(new { message = "Invalid Song ID format." });
            }
            string? youtubeId = await _youTubeClientService.PutYoutubeInstrumentalIdFromSongIdAsync(songGuid);
            if (youtubeId == null)
            {
                return NotFound(new { message = "Instrumental Song not found." });
            }
            return Ok(youtubeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating song video instrumental status for song ID: {SongId}", songId);
            return StatusCode(500, new { message = "An unexpected error occurred.", error = ex.Message });
        }
    }

    [HttpGet("health")]
    [ProducesResponseType(typeof(string), 200)]
    public IActionResult HealthCheck()
    {
        return Ok(new { message = "Song API is healthy." });
    }

    [DevelopmentOnly]
    [HttpGet("database/song")]
    [ProducesResponseType(typeof(Song), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSongFromDb([FromQuery, Required] string title, [FromQuery, Required] string artist, [FromQuery] string? albumName)
    {
        //there is no case for album name but no artist, maybe add in future or not need. This for dev testing
        Song? song = null;
        if (!string.IsNullOrEmpty(albumName) && !string.IsNullOrEmpty(artist))
        {
            song = await _songRepo.GetSongAsync(title, artist, albumName);
        }
        else if (!string.IsNullOrEmpty(artist))
        {
            song = await _songRepo.GetSongAsync(title, artist);
        }
        song ??= await _songRepo.GetSongAsync(title);
        if (song == null)
        {
            return NotFound(new { message = "Song not found in database." });
        }
        return Ok(song);
    }

    [DevelopmentOnly]
    [HttpPost("database/song")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> AddSongToDb([FromBody] Song song)
    {
        await _songRepo.AddSongAsync(song);
        return Ok();
    }

    [DevelopmentOnly]
    [HttpGet("database/users/{email}")]
    [ProducesResponseType(typeof(User), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        User? user = await _userRepo.GetUserByEmailAsync(email);
        return user != null ? Ok(user) : NotFound();
    }
}
