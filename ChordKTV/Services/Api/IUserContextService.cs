using ChordKTV.Models.UserData;
using System.Threading.Tasks;

namespace ChordKTV.Services.Api;

public interface IUserContextService
{
    Task<(User? user, string accessToken, string refreshToken)> AuthenticateGoogleUserAndIssueTokensAsync(string idToken);
    Task<(User? user, string accessToken, string refreshToken)> RefreshTokenAsync(string refreshToken);
    Task<User?> GetCurrentUserAsync();
}
