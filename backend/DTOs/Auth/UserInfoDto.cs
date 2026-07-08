namespace QM_AI.API.DTOs.Auth;

public class UserInfoDto
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Avatar { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public DateTime CreatedAt { get; set; }
}
