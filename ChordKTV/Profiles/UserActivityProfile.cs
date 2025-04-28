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
        CreateMap<UserSongActivity, UserSongActivityDto>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Song.Title))
            .ForMember(dest => dest.Artist, opt => opt.MapFrom(src => src.Song.Artist))
            .ForMember(dest => dest.GeniusThumbnailUrl, opt => opt.MapFrom(src => src.Song.GeniusMetaData != null ? src.Song.GeniusMetaData.SongImageThumbnailUrl : null));
        CreateMap<UserPlaylistActivity, UserPlaylistActivityDto>().ReverseMap();
        CreateMap<UserHandwritingResultDto, UserHandwritingResult>().ReverseMap();
        CreateMap<LearnedWord, LearnedWordDto>().ReverseMap();
        CreateMap<UserPlaylistActivityFavoriteRequestDto, UserPlaylistActivity>();
        CreateMap<UserSongActivityFavoriteRequestDto, UserSongActivity>();
    }
}
