using chakish_2._0_api.Data_Base;
using chakish_2._0_api.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Storage;

namespace chakish_2._0_api.SignalRConroller;

public class ChatHub : Hub
{
    private readonly DataService _dataService;
    private readonly KrolikMQ _krolikMq;

    public ChatHub(DataService dataService, KrolikMQ krolikMq)
    {
        _dataService = dataService;
        _krolikMq = krolikMq;
    }
    public async Task SendMessageToChat(Guid chatId, Guid userId, string message)
    {
        _krolikMq.SendMessage(message, chatId, userId);
        await _dataService.Messages
            .AddAsync(new Message
            {
                ChatId = chatId, 
                UserId = userId, 
                DateTime = DateTime.UtcNow.AddHours(3), 
                Text = message
            });
        await _dataService.SaveChangesAsync();
    }
    
    public async Task JoinChat(Guid chatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
        _krolikMq.StartReceivingMessages(chatId);
    }
    
    public async Task LeaveChat(Guid chatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId.ToString());
    }


}