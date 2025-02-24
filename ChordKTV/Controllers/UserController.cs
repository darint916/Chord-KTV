using Microsoft.AspNetCore.Mvc;
using Google.Apis.Auth;
using Microsoft.Extensions.Logging;
using ChordKTV.Utils.Extensions;
using ChordKTV.Data.Api.UserData;
using ChordKTV.Models.UserData;

namespace ChordKTV.Controllers;

[ApiController]
[Route("api")]
[DevelopmentOnly]
public class UserController : ControllerBase
{
    private readonly string _googleClientId;
    private readonly ILogger<UserController> _logger;
    private readonly IUserRepo _userRepo;

    public UserController(
        IConfiguration configuration,
        ILogger<UserController> logger,
        IUserRepo userRepo)
    {
        _googleClientId = configuration["Authentication:Google:ClientId"] ??
            throw new ArgumentNullException(nameof(configuration), "Google Client ID is not configured");
        _logger = logger;
        _userRepo = userRepo;
    }

    [HttpPost("auth/google")]
    public async Task<IActionResult> AuthenticateGoogle([FromHeader] string authorization)
    {
        try
        {
            _logger.LogDebug("Received authentication request");
            string idToken = authorization.Replace("Bearer ", "");

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

            return Ok(user);
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "Invalid JWT token received");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during authentication");
            return StatusCode(500, new { message = "An unexpected error occurred" });
        }
    }
}
