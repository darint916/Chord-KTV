using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Google.Apis.Auth;
using ChordKTV.Utils.Extensions;
using ChordKTV.Services.Api;
using ChordKTV.Models.UserData;
using ChordKTV.Dtos;
using ChordKTV.Dtos.Auth;
using AutoMapper;
using AutoMapper.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ChordKTV.Controllers;

[ApiController]
[Route("api")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserContextService _userContextService;
    private readonly IMapper _mapper;

    public UserController(
        ILogger<UserController> logger,
        IUserContextService userContextService,
        IMapper mapper)
    {
        _logger = logger;
        _userContextService = userContextService;
        _mapper = mapper;
    }

    [HttpPost("auth/google")]
    [ProducesResponseType(typeof(AuthResponseDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> AuthenticateGoogle([FromHeader] string authorization)
    {
        try
        {
            string idToken = authorization.Replace("Bearer ", "");
            (User user, string accessToken, string refreshToken) = await _userContextService.AuthenticateGoogleUserAndIssueTokensAsync(idToken);

            if (user == null)
            {
                return Unauthorized();
            }

            var responseDto = new AuthResponseDto
            {
                User = _mapper.Map<UserDto>(user),
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
            return Ok(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during authentication");
            return StatusCode(500, new { message = "An unexpected error occurred" });
        }
    }

    [HttpPost("auth/refresh")]
    [ProducesResponseType(typeof(AuthResponseDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
    {
        (User user, string accessToken, string refreshToken) = await _userContextService.RefreshTokenAsync(dto.RefreshToken);

        if (user == null)
        {
            return Unauthorized();
        }

        var responseDto = new AuthResponseDto
        {
            User = _mapper.Map<UserDto>(user),
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
        return Ok(responseDto);
    }
}
