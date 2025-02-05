namespace ChordKTV.Services.Api;

using System.Threading.Tasks;
using ChordKTV.Dtos;

public interface IYouTubeClientService
{
    public Task<PlaylistDetailsDto?> GetPlaylistDetailsAsync(string playlistId);
}
