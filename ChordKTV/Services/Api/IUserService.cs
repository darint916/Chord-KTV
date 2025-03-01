using ChordKTV.Models.UserData;

namespace ChordKTV.Services.Api;

public interface IUserService
{
    Task<User?> AuthenticateGoogleUserAsync(string idToken);
}
