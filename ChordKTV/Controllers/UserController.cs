using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ChordKTV.Utils.Extensions;
using ChordKTV.Services.Api;
using ChordKTV.Models.UserData;
using ChordKTV.Dtos;
using AutoMapper;
using AutoMapper.Configuration;

namespace ChordKTV.Controllers;

[ApiController]
[Route("api")]
[DevelopmentOnly]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserService _userService;
    private readonly Mapper _mapper;

    public UserController(
        ILogger<UserController> logger,
        IUserService userService,
        IMapper mapper)
    {
        _logger = logger;
        _userService = userService;
        
        MapperConfiguration config = new MapperConfiguration(cfg => cfg.CreateMap<User, UserDto>());
        _mapper = new Mapper(config);
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

            UserDto userDto = _mapper.Map<UserDto>(user);
            return Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during authentication");
            return StatusCode(500, new { message = "An unexpected error occurred" });
        }
    }
}
