using AutoMapper;
using ChordKTV.Dtos.UserActivity;
using ChordKTV.Models.Quiz;
using ChordKTV.Models.Playlist;
using ChordKTV.Models.SongData;
using ChordKTV.Models.Handwriting;

namespace ChordKTV.Profiles;

public class UserActivityProfile : Profile
{
    public UserActivityProfile()
    {
        CreateMap<UserQuizResultDto, UserQuizResult>();
        CreateMap<UserQuizResult, UserQuizResultDto>();

        // Map the unified song activity
        CreateMap<UserSongActivity, UserSongActivityDto>()
            .ForMember(dest => dest.DatesPlayed, opt => opt.MapFrom(src => src.DatesPlayed))
            .ReverseMap();

        // Map the unified playlist activity
        CreateMap<UserPlaylistActivity, UserPlaylistActivityDto>()
            .ForMember(dest => dest.DatesPlayed, opt => opt.MapFrom(src => src.DatesPlayed))
            .ReverseMap();

        CreateMap<UserHandwritingResultDto, UserHandwritingResult>();
        CreateMap<UserHandwritingResult, UserHandwritingResultDto>();
    }
}
