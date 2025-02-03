namespace ChordKTV.Controllers;

using System.Formats.Asn1;
using Microsoft.AspNetCore.Mvc;
using ChordKTV.Services.Api;

[ApiController]
[Route("api/song")]
public class SongController : Controller
{
    // private readonly IGeniusService _geniusRepo;
    private readonly ILRCService _lRCService;
    //ADD your interfaces here, this is how you call your services
    //TODO: Add mapper here later (for dto mapping)
    public SongController(ILRCService lRCService) => _lRCService = lRCService;

    // [HttpGet("genius/song/{title:string}")]
    // public async Task<IActionResult> GetSongByArtistTitle(string title, string? artist)
    // {
    //     //var song = await _geniusRepo.GetSongByArtistTitle(title, artist);
    //     //return Ok(song);
    // }

    [HttpGet("lrclib")]
    public async Task<IActionResult> GetLRCLIBLyrics([FromQuery] string title, [FromQuery] string artist)
    {
        if (string.IsNullOrEmpty(title))
        {
            return BadRequest("Track title is required.");
        }

        try
        {
            var lyrics = await _lRCService.GetLRCLIBLyrics(title, artist);
            return Ok(lyrics);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while querying LRCLIB.", error = ex.Message });
        }
    }
}
