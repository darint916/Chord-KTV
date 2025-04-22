using ChordKTV.Models.UserData;
using ChordKTV.Data.Api.UserData;
using ChordKTV.Services.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Google.Apis.Auth;

namespace ChordKTV.Services.Service;

public class UserContextService : IUserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserRepo _userRepo;
    private readonly ILogger<UserContextService> _logger;

    private readonly string _googleClientId;

    public UserContextService(
        IConfiguration configuration,
        ILogger<UserContextService> logger,
        IUserRepo userRepo,
        IHttpContextAccessor httpContextAccessor)
    {
        _googleClientId = configuration["Authentication:Google:ClientId"] ??
            throw new ArgumentNullException(nameof(configuration), "Google Client ID is not configured");
        _logger = logger;
        _userRepo = userRepo;
        _httpContextAccessor = httpContextAccessor; 
    }

    public async Task<User?> AuthenticateGoogleUserAsync(string idToken)
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
                    return null;
                }
            }

            return user;
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "Invalid JWT token received");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during authentication");
            return null;
        }
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        var userClaims = _httpContextAccessor.HttpContext?.User;
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