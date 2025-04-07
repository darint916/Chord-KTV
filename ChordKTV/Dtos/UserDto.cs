namespace ChordKTV.Dtos;

public class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> FavoriteSongs { get; set; } = [];
    public List<string> FavoriteArtists { get; set; } = [];
    public List<string> FavoriteAlbums { get; set; } = [];
    public List<string> FavoritePlaylistLinks { get; set; } = [];
    public List<string> SongHistory { get; set; } = [];
    public List<string> PlaylistHistory { get; set; } = [];
}
