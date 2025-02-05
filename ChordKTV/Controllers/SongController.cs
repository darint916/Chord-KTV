namespace ChordKTV.Controllers;

using System;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ChordKTV.Services.Api;
using ChordKTV.Dtos;

[ApiController]
[Route("api/song")]
public class SongController : Controller
{
    // private readonly IGeniusRepo _geniusRepo;
    private readonly IYouTubeClientService _youTubeService;
    private readonly ILrcService _lrcService;

    public SongController(/*IGeniusRepo geniusRepo,*/ IYouTubeClientService youTubeService, ILrcService lRCService)
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
}
