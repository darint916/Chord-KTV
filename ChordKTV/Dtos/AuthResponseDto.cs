namespace ChordKTV.Dtos;

public class AuthResponseDto
{
    public UserDto User { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
}
