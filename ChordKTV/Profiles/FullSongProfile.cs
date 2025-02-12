using AutoMapper;
using ChordKTV.Models.SongData;
using ChordKTV.Dtos.FullSong;

namespace ChordKTV.Profiles;

public class FullSongProfile : Profile
{
    public FullSongProfile()
    {
        CreateMap<Song, FullSongResponseDto>()
            .ForCtorParam("AlbumNames", opt => opt.MapFrom(src => src.Albums.Select(a => a.Name).ToList()));
    }
}
