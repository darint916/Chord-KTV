namespace ChordKTV.Controllers;

using Microsoft.AspNetCore.Mvc;
using ChordKTV.Services.Api;
using System.ComponentModel.DataAnnotations;

[ApiController]
[Route("api/song")]
public class SongController : Controller
{
    // private readonly IGeniusService _geniusRepo;
    private readonly ILRCService _lRCService;
    //ADD your interfaces here, this is how you call your services
    //TODO: Add mapper here later (for dto mapping)
    public SongController(ILRCService lRCService)
    {
        _lRCService = lRCService;
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
        string? lyrics = await _lRCService.GetLRCLIBLyrics(title, artist, albumName, duration);

        if (lyrics == null)
        {
            return NotFound(new { message = "Lyrics not found for the specified track." });
        }

        return Ok(lyrics);
    }
}
