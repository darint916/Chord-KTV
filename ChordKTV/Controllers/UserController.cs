using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Google.Apis.Auth;
using ChordKTV.Utils.Extensions;
using ChordKTV.Services.Api;
using ChordKTV.Models.UserData;
using ChordKTV.Dtos;
using AutoMapper;
using AutoMapper.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ChordKTV.Controllers;

[ApiController]
[Route("api")]
[DevelopmentOnly]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserService _userService;
    private readonly Mapper _mapper;
    private readonly IConfiguration _configuration;

    public UserController(
        ILogger<UserController> logger,
        IUserService userService,
        IMapper mapper,
        IConfiguration configuration)
    {
        _logger = logger;
        _userService = userService;

        MapperConfiguration config = new MapperConfiguration(cfg => cfg.CreateMap<User, UserDto>());
        _mapper = new Mapper(config);
        _configuration = configuration;
    }

    [HttpPost("auth/google")]
    public async Task<IActionResult> AuthenticateGoogle([FromHeader] string authorization)
    {
        try
        {
            string idToken = authorization.Replace("Bearer ", "");
            User? user = await _userService.AuthenticateGoogleUserAsync(idToken);

            if (user == null)
            {
                return Unauthorized();
            }

            // Just return the user and let the client continue using the Google token
            return Ok(new { 
                user = _mapper.Map<UserDto>(user),
                token = idToken  // Return the same Google token
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during authentication");
            return StatusCode(500, new { message = "An unexpected error occurred" });
        }
    }
}
