namespace ChordKTV.Controllers;

using System;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ChordKTV.Services.Api;
using ChordKTV.Dtos;
using ChordKTV.Data.Api.SongData;
using ChordKTV.Models.SongData;
using ChordKTV.Utils.Extensions;

[ApiController]
[Route("api")]
public class SongController : Controller
{
    private readonly ISongRepo _songRepo;
    private readonly IYouTubeClientService _youTubeService;
    private readonly ILrcService _lrcService;
    private readonly IChatGptService _chatGptService;

    public SongController(/*IGeniusRepo geniusRepo,*/ IYouTubeClientService youTubeService, ILrcService lrcService, ISongRepo songRepo, IChatGptService chatGptService)
    {
        // _geniusRepo = geniusRepo;
        _songRepo = songRepo;
        _youTubeService = youTubeService;
        _lrcService = lrcService;
        _chatGptService = chatGptService;
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
    public async Task<IActionResult> GetLrcLibLyrics([FromQuery, Required] string title, [FromQuery] string? artist,
                                    [FromQuery] string? albumName)
    {
        try
        {
            LrcLyricsDto? lyrics = await _lrcService.GetLrcLibLyricsAsync(title, artist, albumName);

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
}
