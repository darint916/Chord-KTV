using Microsoft.AspNetCore.Mvc;
using Google.Apis.Auth;

[ApiController]
[Route("api")]
public class UserController : ControllerBase
{
    private readonly string _googleClientId;
    
    public UserController(IConfiguration configuration)
    {
        _googleClientId = configuration["Authentication:Google:ClientId"];
    }
    
    [HttpPost("random")]
    public async Task<IActionResult> AddSearchHistory([FromHeader] string authorization)
    {
        try {
            // 1. Extract token from Authorization header
            string idToken = authorization.Replace("Bearer ", "");

            // 2. Verify the token with Google
            var validationSettings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _googleClientId }
            };
            
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);
            
            // 3. Now we can trust the user ID (payload.Subject)
            // Process the request...
            Console.WriteLine(payload.Subject);
            
            return Ok();
        }
        catch (InvalidJwtException)
        {
            Console.WriteLine("Invalid JWT");
            return Unauthorized();
        }
    }
} 