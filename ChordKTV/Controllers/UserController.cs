using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ChordKTV.Utils.Extensions;
using ChordKTV.Services.Api;
using ChordKTV.Models.UserData;

namespace ChordKTV.Controllers;

[ApiController]
[Route("api")]
[DevelopmentOnly]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserService _userService;

    public UserController(
        ILogger<UserController> logger,
        IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    [HttpPost("auth/google")]
    public async Task<IActionResult> AuthenticateGoogle([FromHeader] string authorization)
    {
        try
        {
            _logger.LogDebug("Received authentication request");
            string idToken = authorization.Replace("Bearer ", "");

            User? user = await _userService.AuthenticateGoogleUserAsync(idToken);
            
            if (user == null)
            {
                return Unauthorized();
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during authentication");
            return StatusCode(500, new { message = "An unexpected error occurred" });
        }
    }
}
