namespace ChordKTV.Services.Api;

using System.Threading.Tasks;
using ChordKTV.Models.ApiModels;

public interface IYouTubeService
{
    public Task<PlaylistDetailsDto> GetPlaylistDetailsAsync(string playlistId);
}
