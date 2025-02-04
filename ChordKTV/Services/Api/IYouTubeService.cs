namespace ChordKTV.Services.Api;

using System.Threading.Tasks;
using ChordKTV.Models.ApiModels;

public interface IYouTubeService
{
    Task<PlaylistDetailsDto> GetPlaylistDetails(string playlistId);
}