using chakish_2._0_api.Data_Base;
using chakish_2._0_api.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace chakish_2._0_api.SignalRConroller;

public class ChatHub : Hub
{
    private readonly DataService _dataService;

    public ChatHub(DataService dataService)
    {
        _dataService = dataService;
    }
    
    public async Task SendMessageToChat(Guid chatId, string userId, string message)
    {
        var newMessage = new Message
        {
            ChatId = chatId,
            UserId = userId,
            DateTime = DateTime.UtcNow.AddHours(3),
            Text = message
        };
        
        await _dataService.Messages.AddAsync(newMessage);
        await _dataService.SaveChangesAsync();
        
        var user = await _dataService.Users.FindAsync(userId);
        var userName = user?.Name ?? "Невідомий користувач";
        
        var messageData = new
        {
            Id = newMessage.Id,
            ChatId = chatId,
            UserId = userId,
            UserName = userName,
            Text = message,
            DateTime = newMessage.DateTime
        };
        
        // Відправляємо повідомлення всім підключеним клієнтам у групі чату
        await Clients.Group(chatId.ToString()).SendAsync("ReceiveMessage", messageData);
    }
    
    // Метод для приєднання до чату
    public async Task JoinChat(Guid chatId)
    {
        // Додаємо користувача до групи чату
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
        
        // Отримуємо ID користувача з контексту підключення
        var userId = GetUserIdFromConnection();
        if (userId != Guid.Empty)
        {
            // Отримуємо ім'я користувача
            var user = await _dataService.Users.FindAsync(userId);
            var userName = user?.Name ?? "Невідомий користувач";
            
            // Повідомляємо інших користувачів чату про нового учасника
            await Clients.Group(chatId.ToString()).SendAsync("UserJoined", new { UserId = userId, UserName = userName });
        }
    }
    
    // Метод для виходу з чату
    public async Task LeaveChat(Guid chatId)
    {
        // Видаляємо користувача з групи чату
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId.ToString());
        
        // Отримуємо ID користувача з контексту підключення
        var userId = GetUserIdFromConnection();
        if (userId != Guid.Empty)
        {
            // Повідомляємо інших користувачів чату про вихід учасника
            await Clients.Group(chatId.ToString()).SendAsync("UserLeft", new { UserId = userId });
        }
    }
    
    // Метод для сповіщення про друкування
    public async Task UserIsTyping(Guid chatId, Guid userId)
    {
        // Надсилаємо сповіщення всім користувачам, крім самого користувача, який друкує
        await Clients.GroupExcept(chatId.ToString(), Context.ConnectionId)
            .SendAsync("UserTyping", new { UserId = userId });
    }
    
    // Метод для оновлення статусу прочитання повідомлення
    public async Task MarkMessageAsRead(Guid messageId, Guid userId)
    {
        // Знаходимо повідомлення
        var message = await _dataService.Messages.FindAsync(messageId);
        if (message != null)
        {
            message.IsRead = true; // Припускаємо, що в моделі є таке поле
            await _dataService.SaveChangesAsync();
            
            await Clients.Group(message.ChatId.ToString())
                .SendAsync("MessageRead", new { MessageId = messageId, UserId = userId });
        }
    }
    
    public async Task<List<object>> GetChatHistory(Guid chatId, int skip = 0, int take = 50)
    {
        var messages = await _dataService.Messages.Where(m => m.ChatId == chatId).Skip(skip).Take(take).ToListAsync();
        
        return messages.Select(m => new
        {
            Id = m.Id,
            ChatId = m.ChatId,
            UserId = m.UserId,
            Text = m.Text,
            DateTime = m.DateTime,
            IsRead = m.IsRead
        }).ToList<object>();
    }
    
    private Guid GetUserIdFromConnection()
    {
        var userIdClaim = Context.User?.FindFirst("sub")?.Value;
        if (Guid.TryParse(userIdClaim, out Guid userId))
        {
            return userId;
        }
        return Guid.Empty;
    }
    
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserIdFromConnection();
        if (userId != Guid.Empty)
        {
            await UpdateUserStatus(userId, true);
        }
        
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserIdFromConnection();
        if (userId != Guid.Empty)
        {
            await UpdateUserStatus(userId, false);
        }
        await base.OnDisconnectedAsync(exception);
    }
    
    private async Task UpdateUserStatus(Guid userId, bool isOnline)
    {
        var user = await _dataService.Users.FindAsync(userId);
        if (user != null)
        {
            // Припускаємо, що в моделі User є поле IsOnline
            user.IsOnline = isOnline;
            user.LastSeen = DateTime.UtcNow.AddHours(3);
            
            await _dataService.SaveChangesAsync();
            
            // Сповіщаємо всіх про зміну статусу користувача
            // (потрібно адаптувати під вашу логіку груп та чатів)
            await Clients.All.SendAsync("UserStatusChanged", new { UserId = userId, IsOnline = isOnline });
        }
    }
}