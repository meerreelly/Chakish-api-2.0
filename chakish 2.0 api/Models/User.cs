namespace chakish_2._0_api.Models;

public class User
{
    public Guid Id { get; set; }
    public string Role { get; set; } = "user";
    public List<string> Photos { get; set; } = new List<string>();
    public string GoogleId { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string AvatarUrl { get; set; } = "test";

    public User(string googleId, string email, string name)
    {
        GoogleId = googleId;
        Email = email;
        Name = name;
    }

    public User()
    {
        
    }
}