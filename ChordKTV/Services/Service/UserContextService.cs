using ChordKTV.Models.UserData;
using ChordKTV.Data.Api.UserData;
using ChordKTV.Services.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Google.Apis.Auth;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace ChordKTV.Services.Service;

public class UserContextService : IUserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserRepo _userRepo;
    private readonly ILogger<UserContextService> _logger;

    private readonly string _googleClientId;
    private readonly string _jwtKey;

    public UserContextService(
        IConfiguration configuration,
        ILogger<UserContextService> logger,
        IUserRepo userRepo,
        IHttpContextAccessor httpContextAccessor)
    {
        _googleClientId = configuration["Authentication:Google:ClientId"] ??
            throw new ArgumentNullException(nameof(configuration), "Google Client ID is not configured");
        _jwtKey = configuration["Jwt:Key"] ??
            throw new ArgumentNullException(nameof(configuration), "JWT Key is not configured");
        _logger = logger;
        _userRepo = userRepo;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<(User? user, string accessToken, string refreshToken)> AuthenticateGoogleUserAndIssueTokensAsync(string idToken)
    {
        try
        {
            GoogleJsonWebSignature.ValidationSettings validationSettings = new()
            {
                Audience = [_googleClientId]
            };

            GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);

            // Check if user exists
            User? user = await _userRepo.GetUserByEmailAsync(payload.Email);

            if (user == null)
            {
                // Create new user
                user = new User
                {
                    Email = payload.Email,
                    Name = payload.Name
                };
                await _userRepo.CreateUserAsync(user);
                bool saved = await _userRepo.SaveChangesAsync();

                if (!saved)
                {
                    _logger.LogWarning("Failed to save new user: {Email}", payload.Email);
                    return (null, "", "");
                }
            }

            // Issue tokens
            string accessToken = GenerateJwtToken(user);
            string refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);
            await _userRepo.SaveChangesAsync();

            return (user, accessToken, refreshToken);
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "Invalid JWT token received");
            return (null, "", "");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during authentication");
            return (null, "", "");
        }
    }

    public string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        byte[] key = Encoding.UTF8.GetBytes(_jwtKey);

        Claim[] claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Name, user.Name ?? "")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }

    public async Task<(User? user, string accessToken, string refreshToken)> RefreshTokenAsync(string refreshToken)
    {
        User? user = await _userRepo.GetUserByRefreshTokenAsync(refreshToken);
        if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
        {
            return (null, "", "");
        }

        string newAccessToken = GenerateJwtToken(user);
        string newRefreshToken = GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);
        await _userRepo.SaveChangesAsync();

        return (user, newAccessToken, newRefreshToken);
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        ClaimsPrincipal? userClaims = _httpContextAccessor.HttpContext?.User;
        if (userClaims == null)
        {
            _logger.LogWarning("No user claims available in HttpContext");
            return null;
        }

        _logger.LogInformation("Claims present: {Claims}",
            string.Join(", ", userClaims.Claims.Select(c => $"{c.Type}: {c.Value}")));

        string? email = userClaims.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("No email claim found in token");
            return null;
        }

        User? user = await _userRepo.GetUserByEmailAsync(email);
        if (user == null)
        {
            _logger.LogWarning("User not found for email: {Email}", email);
        }

        return user;
    }
}
