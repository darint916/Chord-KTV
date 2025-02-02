namespace ChordKTV.Controllers;

using System.Formats.Asn1;
using ChordKTV.Services.Api;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/song")]
public class SongController : Controller
{
    // private readonly IGeniusService _geniusRepo;
    private readonly ILRCService _lRCService;
    //ADD your interfaces here, this is how you call your services
    //TODO: Add mapper here later (for dto mapping)
    public SongController(ILRCService lRCService) => this._lRCService = lRCService;

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
            return this.BadRequest("Track title is required.");
        }

        try
        {
            var lyrics = await this._lRCService.GetLRCLIBLyrics(title, artist);
            return this.Ok(lyrics);
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, new { message = "An error occurred while querying LRCLIB.", error = ex.Message });
        }
    }
}
