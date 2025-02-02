namespace ChordKTV.Services.Api;
public interface ILRCService
{
    public Task<LyricsDetails> GetLRCLIBLyrics(string title, string artist);
    public class LyricsDetails
    {
        public int Id { get; set; }
        public required string TrackName { get; set; }
        public required string ArtistName { get; set; }
        public required string AlbumName { get; set; }
        public float Duration { get; set; }
        public bool Instrumental { get; set; }
        public required string PlainLyrics { get; set; }
        public required string SyncedLyrics { get; set; }
    }
}
