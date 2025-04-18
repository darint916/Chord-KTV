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
            .ForMember(dest => dest.PlayDates, opt => opt.MapFrom(src => src.PlayDates))
            .ReverseMap();

        // Map the unified playlist activity
        CreateMap<UserPlaylistActivity, UserPlaylistActivityDto>()
            .ForMember(dest => dest.PlayDates, opt => opt.MapFrom(src => src.PlayDates))
            .ReverseMap();

        CreateMap<UserHandwritingResultDto, UserHandwritingResult>();
        CreateMap<UserHandwritingResult, UserHandwritingResultDto>();
    }
}
