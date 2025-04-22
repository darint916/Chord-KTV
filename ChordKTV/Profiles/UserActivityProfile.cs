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
        CreateMap<UserQuizResultDto, UserQuizResult>().ReverseMap();
        CreateMap<UserSongActivity, UserSongActivityDto>().ReverseMap();
        CreateMap<UserPlaylistActivity, UserPlaylistActivityDto>().ReverseMap();
        CreateMap<UserHandwritingResultDto, UserHandwritingResult>().ReverseMap();
        CreateMap<LearnedWord, LearnedWordDto>().ReverseMap();
    }
}
