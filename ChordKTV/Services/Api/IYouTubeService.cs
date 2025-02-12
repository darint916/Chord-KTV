using ChordKTV.Dtos;

namespace ChordKTV.Services.Api;

public interface IYouTubeClientService
{
    public Task<PlaylistDetailsDto?> GetPlaylistDetailsAsync(string playlistId);
}
