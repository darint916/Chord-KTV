using ChordKTV.Models.SongData;

namespace ChordKTV.Data.Api.SongData;

public interface IYoutubeSongRepo
{
    public Task<YoutubeSong?> GetYoutubeSongByYoutubeId(string id);
    public Task<Song?> GetSongByYoutubeId(string id)
    public Task AddYoutubeSongAsync(YoutubeSong song);
}
