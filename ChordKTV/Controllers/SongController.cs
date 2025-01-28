namespace ChordKTV.Controllers;

using ChordKTV.Services.Api;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/song")]
public class SongController : Controller
{
    private readonly IGeniusRepo _geniusRepo;
    //ADD your interfaces here, this is how you call your services
    //TODO: Add mapper here later (for dto mapping)

    [HttpGet("genius/{song:string}")]
    public async Task<IActionResult> GetSongByArtistTitle(string title, string? artist)
    {
        //var song = await _geniusRepo.GetSongByArtistTitle(title, artist);
        //return Ok(song);
    }
}
