using AutoMapper;
using ChordKTV.Models.SongData;
using ChordKTV.Dtos;
using ChordKTV.Dtos.GeniusApi;

namespace ChordKTV.Profiles;

public class SongProfile : Profile
{
    public SongProfile()
    {
        CreateMap<Song, SongDto>()
            .ForCtorParam("Id", opt => opt.MapFrom(src => src.Id))
            .ForCtorParam("AlbumNames", opt => opt.MapFrom(src => src.Albums.Select(a => a.Name).ToList()));
        CreateMap<GeniusMetaData, GeniusMetaDataDto>();
    }
}
