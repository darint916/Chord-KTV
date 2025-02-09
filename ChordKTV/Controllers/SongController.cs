namespace ChordKTV.Controllers;

using System;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ChordKTV.Services.Api;
using ChordKTV.Dtos;
using ChordKTV.Data.Api.SongData;
using ChordKTV.Models.SongData;
using ChordKTV.Utils.Extensions;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;

[ApiController]
[Route("api")]
public class SongController : Controller
{
    private readonly ISongRepo _songRepo;
    private readonly IYouTubeClientService _youTubeService;
    private readonly ILrcService _lrcService;
    private readonly IGeniusService _geniusService;
    private readonly IAlbumRepo _albumRepo;
    private readonly ILogger<SongController> _logger;
    private readonly IMapper _mapper;
    private readonly IChatGptService _chatGptService;

    public SongController(
        IMapper mapper,
        IYouTubeClientService youTubeService,
        ILrcService lrcService,
        ISongRepo songRepo,
        IGeniusService geniusService,
        IAlbumRepo albumRepo,
        IChatGptService chatGptService,
        ILogger<SongController> logger)
    {
        _mapper = mapper;
        _songRepo = songRepo;
        _youTubeService = youTubeService;
        _lrcService = lrcService;
        _geniusService = geniusService;
        _albumRepo = albumRepo;
        _chatGptService = chatGptService;
        _logger = logger;
    }

    // [HttpGet("genius/{song:string}")]

    // public async Task<IActionResult> GetSongByArtistTitle(string title, string? artist)
    // {
    //     //var song = await _geniusRepo.GetSongByArtistTitle(title, artist);
    //     //return Ok(song);
    // }

    [HttpGet("youtube/playlist/{playlistId}")]
    public async Task<IActionResult> GetYouTubePlaylist(string playlistId)
    {
        PlaylistDetailsDto? result = await _youTubeService.GetPlaylistDetailsAsync(playlistId);
        if (result == null)
        {
            return StatusCode(500, new { message = "Server error: YouTube API key is missing." });
        }

        return Ok(result);
    }

    [HttpGet("lrclib/search")]
    public async Task<IActionResult> GetLrcLibLyrics([FromQuery, Required] string title, [FromQuery, Required] string artist,
                                    [FromQuery] string? albumName, [FromQuery] float? duration)
    {
        try
        {
            string? lyrics = await _lrcService.GetLrcLibLyricsAsync(title, artist, albumName, duration);

            if (lyrics == null)
            {
                return NotFound(new { message = "Lyrics not found for the specified track." });
            }

            return Ok(lyrics);
        }
        catch (HttpRequestException ex) // Handles HTTP request errors
        {
            return StatusCode(503, new { message = "Failed to fetch lyrics. Service may be unavailable.", error = ex.Message });
        }
        catch (Exception ex) // Handles unexpected errors
        {
            return StatusCode(500, new { message = "An unexpected error occurred.", error = ex.Message });
        }
    }

    [HttpPost("chatgpt/translation")]
    public async Task<IActionResult> PostChatGptTranslations([FromBody] TranslationRequestDto request)
    {
        TranslationResponseDto lyricsDto = await _chatGptService.TranslateLyricsAsync(request.OriginalLyrics, request.LanguageCode, request.Romanize, request.Translate);
        return Ok(lyricsDto);
    }

    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new { message = "Song API is healthy." });
    }

    [DevelopmentOnly]
    [HttpGet("database/song")]
    public async Task<IActionResult> GetSongFromDb([FromQuery, Required] string title, [FromQuery] string? artist, [FromQuery] string? albumName)
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
    public async Task<IActionResult> AddSongToDb([FromBody] Song song)
    {
        await _songRepo.AddAsync(song);
        return Ok();
    }

    [HttpGet("genius/search")]
    public async Task<IActionResult> GetSongByArtistTitle(
        [FromQuery, Required] string title,
        [FromQuery] string? artist,
        [FromQuery] bool forceRefresh = false)
    {
        try
        {
            Song? song = await _geniusService.GetSongByArtistTitleAsync(title, artist, forceRefresh);
            if (song == null)
            {
                return NotFound(new { message = "Song not found on Genius." });
            }

            return Ok(_mapper.Map<SongDto>(song));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch from Genius API for title: {Title}, artist: {Artist}", title, artist);
            return StatusCode(503, new { message = "Failed to fetch from Genius API.", error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching song. Title: {Title}, Artist: {Artist}", title, artist);
            return StatusCode(500, new { message = "An unexpected error occurred.", error = ex.Message });
        }
    }

    [HttpPost("genius/search/batch")]
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

            List<VideoInfo> videos = videosElement.EnumerateArray()
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
}
