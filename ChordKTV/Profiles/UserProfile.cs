using AutoMapper;
using ChordKTV.Models.UserData;
using ChordKTV.Dtos;

namespace ChordKTV.Profiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>();
    }
}
