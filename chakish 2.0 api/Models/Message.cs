using System;

namespace chakish_2._0_api.Models;

public class Message
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public string Text { get; set; }
    public Guid ChatId { get; set; }
    public DateTime DateTime { get; set; }
    public bool IsRead { get; set; }

    public Message(User user, string text, Chat chat)
    {
        UserId = user.Id;
        ChatId = chat.ChatId;
        Text = text;
        DateTime = DateTime.Now;
    }

    public Message()
    {
    }
}