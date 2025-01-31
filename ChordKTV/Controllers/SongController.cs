namespace ChordKTV.Controllers;

//using ChordKTV.Services.Api;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/song")]
public class SongController : Controller
{
    // private readonly IGeniusRepo _geniusRepo;
    private readonly IYouTubeService _youTubeService;

    //ADD your interfaces here, this is how you call your services
    //TODO: Add mapper here later (for dto mapping)
    public SongController(/*IGeniusRepo geniusRepo,*/ IYouTubeService youTubeService)
    {
        // _geniusRepo = geniusRepo;
        _youTubeService = youTubeService;
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
        var result = await _youTubeService.GetPlaylistDetails(playlistId);
        return Content(result, "application/json");
    }
}
