namespace chakish_2._0_api.Models;

public class Chat
{
    public Guid ChatId { get; set; }
    public List<Guid> Users { get; set; } = new List<Guid>();

    public Chat(Guid user1, Guid user2)
    {
        Users.Add(user1);
        Users.Add(user2);
    }

    public Chat()
    {
        
    }
}