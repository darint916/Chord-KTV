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

        CreateMap<UserPlaylistActivityDto, UserPlaylistActivity>();
        CreateMap<UserPlaylistActivity, UserPlaylistActivityDto>();

        CreateMap<UserSongPlayDto, UserSongPlay>();
        CreateMap<UserSongPlay, UserSongPlayDto>();

        CreateMap<UserHandwritingResultDto, UserHandwritingResult>();
        CreateMap<UserHandwritingResult, UserHandwritingResultDto>();
    }
}
