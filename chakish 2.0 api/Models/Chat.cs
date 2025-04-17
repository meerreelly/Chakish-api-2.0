namespace chakish_2._0_api.Models;

public class Chat
{
    public Guid ChatId { get; set; }
    public List<string> Users { get; set; } = new List<string>();

    public Chat(string user1, string user2)
    {
        Users.Add(user1);
        Users.Add(user2);
    }

    public Chat()
    {
        
    }
}