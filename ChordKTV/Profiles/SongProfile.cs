using AutoMapper;
using ChordKTV.Models.SongData;
using ChordKTV.Dtos;

namespace ChordKTV.Profiles;

public class SongProfile : Profile
{
    public SongProfile()
    {
        CreateMap<Song, SongDto>()
            .ForCtorParam("Name", opt => opt.MapFrom(src => src.Name))
            .ForCtorParam("PrimaryArtist", opt => opt.MapFrom(src => src.PrimaryArtist))
            .ForCtorParam("FeaturedArtists", opt => opt.MapFrom(src => src.FeaturedArtists))
            .ForCtorParam("AlbumNames", opt => opt.MapFrom(src => src.Albums.Select(a => a.Name).ToList()))
            .ForCtorParam("Genre", opt => opt.MapFrom(src => src.Genre))
            .ForCtorParam("PlainLyrics", opt => opt.MapFrom(src => src.PlainLyrics))
            .ForCtorParam("GeniusMetaData", opt => opt.MapFrom(src => src.GeniusMetaData));

        CreateMap<GeniusMetaData, GeniusMetaDataDto>()
            .ForCtorParam("GeniusId", opt => opt.MapFrom(src => src.GeniusId))
            .ForCtorParam("HeaderImageUrl", opt => opt.MapFrom(src => src.HeaderImageUrl))
            .ForCtorParam("SongImageUrl", opt => opt.MapFrom(src => src.SongImageUrl))
            .ForCtorParam("Language", opt => opt.MapFrom(src => src.Language));
    }
}
