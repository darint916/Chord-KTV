using Google.Apis.Auth;
using Microsoft.Extensions.Logging;
using ChordKTV.Data.Api.UserData;
using ChordKTV.Models.UserData;
using ChordKTV.Services.Api;

namespace ChordKTV.Services.Service;

public class UserService : IUserService
{
    private readonly string _googleClientId;
    private readonly ILogger<UserService> _logger;
    private readonly IUserRepo _userRepo;

    public UserService(
        IConfiguration configuration,
        ILogger<UserService> logger,
        IUserRepo userRepo)
    {
        _googleClientId = configuration["Authentication:Google:ClientId"] ??
            throw new ArgumentNullException(nameof(configuration), "Google Client ID is not configured");
        _logger = logger;
        _userRepo = userRepo;
    }

    public async Task<User?> AuthenticateGoogleUserAsync(string idToken)
    {
        try
        {
            _logger.LogDebug("Validating Google token");
            
            GoogleJsonWebSignature.ValidationSettings validationSettings = new()
            {
                Audience = [_googleClientId]
            };

            GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);
            _logger.LogDebug("Successfully validated Google token for email: {Email}", payload.Email);

            // Check if user exists
            User? user = await _userRepo.GetUserByEmailAsync(payload.Email);
            _logger.LogDebug("Database lookup for user {Email} returned: {UserFound}",
                payload.Email, user != null ? "Found" : "Not Found");

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
                _logger.LogDebug("Created new user: ID: {Id}, Email: {Email}, Save successful: {Saved}",
                    user.Id, payload.Email, saved);
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
} 