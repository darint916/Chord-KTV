using Microsoft.AspNetCore.Mvc;
using Google.Apis.Auth;
using ChordKTV.Utils.Extensions;

namespace ChordKTV.Controllers;

[ApiController]
[Route("api")]
[DevelopmentOnly]
public class UserController : ControllerBase
{
    private readonly string _googleClientId;
    private readonly ILogger<UserController> _logger;

    public UserController(IConfiguration configuration, ILogger<UserController> logger)
    {
        _googleClientId = configuration["Authentication:Google:ClientId"] ??
            throw new ArgumentNullException(nameof(configuration), "Google Client ID is not configured");
        _logger = logger;
    }

    [HttpPost("random")]
    public async Task<IActionResult> AddSearchHistory([FromHeader] string authorization)
    {
        try
        {
            // 1. Extract token from Authorization header
            string idToken = authorization.Replace("Bearer ", "");

            // 2. Verify the token with Google
            var validationSettings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_googleClientId]
            };

            GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);

            // 3. Now we can trust the user ID (payload.Subject)
            _logger.LogDebug("Processing request for user {UserId}", payload.Subject);

            return Ok();
        }
        catch (InvalidJwtException)
        {
            _logger.LogWarning("Invalid JWT token received");
            return Unauthorized();
        }
    }
}
