namespace chakish_2._0_api.Models;

public class ChatResponse
{
    public Guid ChatId { get; set; }
    public List<User> Users { get; set; } = new List<User>();
    public List<MessagesResponse> Messages { get; set; }
}