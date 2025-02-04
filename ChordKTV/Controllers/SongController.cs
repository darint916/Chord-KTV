namespace ChordKTV.Controllers;

using Microsoft.AspNetCore.Mvc;
using ChordKTV.Services.Api;
using System.ComponentModel.DataAnnotations;

[ApiController]
[Route("api/song")]
public class SongController : Controller
{
    // private readonly IGeniusService _geniusRepo;
    private readonly ILrcService _lrcService;
    //ADD your interfaces here, this is how you call your services
    //TODO: Add mapper here later (for dto mapping)
    public SongController(ILrcService lRCService)
    {
        _lrcService = lRCService;
    }


    // [HttpGet("genius/song/{title:string}")]
    // public async Task<IActionResult> GetSongByArtistTitle(string title, string? artist)
    // {
    //     //var song = await _geniusRepo.GetSongByArtistTitle(title, artist);
    //     //return Ok(song);
    // }

    [HttpGet("lrclib/search")]
    public async Task<IActionResult> GetLRCLIBLyrics([FromQuery, Required] string title, [FromQuery, Required] string artist,
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
