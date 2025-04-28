using AutoMapper;
using ChordKTV.Dtos.LrcLib;

namespace ChordKTV.Profiles;

public class LrcMetaDataProfile : Profile
{
    public LrcMetaDataProfile()
    {
        CreateMap<LrcLyricsDto, LrcMetaDataDto>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.TrackName))
            .ForMember(dest => dest.Artist, opt => opt.MapFrom(src => src.ArtistName))
            .ForMember(dest => dest.Duration, opt => opt.MapFrom(src =>
                src.Duration.HasValue ? TimeSpan.FromSeconds(src.Duration.Value) : (TimeSpan?)null));
    }
}
