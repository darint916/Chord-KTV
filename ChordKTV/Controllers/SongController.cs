namespace ChordKTV.Controllers;

using System;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ChordKTV.Models.ApiModels;
using ChordKTV.Services.Api;

[ApiController]
[Route("api")]
public class SongController : Controller
{
    // private readonly IGeniusRepo _geniusRepo;
    private readonly IYouTubeService _youTubeService;
    private readonly ILrcService _lrcService;

    public SongController(/*IGeniusRepo geniusRepo,*/ IYouTubeService youTubeService, ILrcService lRCService)
    {
        // _geniusRepo = geniusRepo;
        _youTubeService = youTubeService;
        _lrcService = lRCService;
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

    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new { message = "Song API is healthy." });
    }
}
