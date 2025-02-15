using AutoMapper;
using ChordKTV.Models.SongData;
using ChordKTV.Dtos;

namespace ChordKTV.Profiles;

public class SongProfile : Profile
{
    public SongProfile()
    {
        CreateMap<Song, SongDto>()
            .ForCtorParam("Name", opt => opt.MapFrom(src => src.Title))
            .ForCtorParam("PrimaryArtist", opt => opt.MapFrom(src => src.Artist))
            .ForCtorParam("FeaturedArtists", opt => opt.MapFrom(src => src.FeaturedArtists))
            .ForCtorParam("AlbumNames", opt => opt.MapFrom(src => src.Albums.Select(a => a.Name).ToList()))
            .ForCtorParam("Genre", opt => opt.MapFrom(src => src.Genre))
            .ForCtorParam("PlainLyrics", opt => opt.MapFrom(src => src.PlainLyrics))
            .ForCtorParam("GeniusMetaData", opt => opt.MapFrom(src => src.GeniusMetaData));

        CreateMap<GeniusMetaData, GeniusMetaDataDto>();
    }
}
