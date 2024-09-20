namespace chakish_2._0_api.Models;

public class UserDto
{
    public Guid Id { get; set; }
    public string Role { get; set; } = "user";
    public string? Name { get; set; }
    public string? AvatarUrl { get; set; }
}