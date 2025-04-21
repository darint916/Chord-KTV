using ChordKTV.Models.SongData;

namespace ChordKTV.Data.Api.SongData;

public interface IYoutubeSongRepo
{
    public Task<YoutubeSong?> GetYoutubeSongByYoutubeIdAsync(string id);
    public Task<Song?> GetSongByYoutubeIdAsync(string id);
    public Task AddYoutubeSongAsync(YoutubeSong youtubeSong);
}
