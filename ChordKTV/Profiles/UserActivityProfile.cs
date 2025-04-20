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

        CreateMap<UserSongActivity, UserSongActivityDto>();

        CreateMap<UserPlaylistActivity, UserPlaylistActivityDto>();

        CreateMap<UserHandwritingResultDto, UserHandwritingResult>();
        CreateMap<UserHandwritingResult, UserHandwritingResultDto>();

        CreateMap<LearnedWord, LearnedWordDto>().ReverseMap();
    }
}
