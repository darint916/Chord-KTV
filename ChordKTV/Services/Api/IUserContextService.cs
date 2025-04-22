using ChordKTV.Models.UserData;
using System.Threading.Tasks;

namespace ChordKTV.Services.Api;

public interface IUserContextService
{
    Task<User?> AuthenticateGoogleUserAsync(string idToken);
    Task<User?> GetCurrentUserAsync();
}
