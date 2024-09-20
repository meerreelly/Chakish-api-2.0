using System.Security.Claims;
using chakish_2._0_api.Data_Base;
using chakish_2._0_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace chakish_2._0_api.Controllers;


[ApiController]
public class ChatController(DataService dbContext, IConfiguration configuration, KrolikMQ krolik) : ControllerBase
{
    private readonly DataService _dbContext = dbContext;
    private readonly IConfiguration _configuration = configuration;
    private KrolikMQ _krolikMq = krolik;
    
    [Authorize]
    [HttpGet("/find-user")]
    public IActionResult GetUserInfo([FromQuery] string serachTag)
    {
        var googleId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var user = _dbContext.Users
            .Where(u=>u.Name.ToLower().Contains(serachTag.ToLower())
                      && !u.GoogleId.Equals(googleId))
            .ToListAsync();
        if (user.Result.Count==0)
        {
            return NotFound();
        }
        return Ok(user.Result);
    }
    [Authorize]
    [HttpGet("/get-chat-list")]
    public async Task<IActionResult> GetChatList()
    {
            var value = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            var userIdClaim = Guid.Parse(value);
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userIdClaim);
            var chats = await _dbContext.Chats
                .Where(chat => chat.Users.Contains(userIdClaim))
                .ToListAsync();
            var response = new List<ChatResponse>();
            foreach (var chat in chats)
            {
                var secondUserId = chat.Users.FirstOrDefault(e => e != userIdClaim);
                var secondUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == secondUserId);
                var users = new List<User> { user, secondUser };
                response.Add(new ChatResponse { Users = users, ChatId = chat.ChatId});
            }

            return Ok(response);
        
    }

    
    [Authorize]
    [HttpGet("/get-chat")]
    public IActionResult GetChatList([FromQuery] Guid? chatId, Guid? userId)
    {
        var requestedUser = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);
        
        if (chatId.ToString().IsNullOrEmpty() && !userId.ToString().IsNullOrEmpty())
        {
            var existingChat =  dbContext.Chats
                .FirstOrDefaultAsync(c => c.Users
                    .Contains(userId.Value) && c.Users.Contains(requestedUser));
            if (existingChat.Result !=default)
            {
                /*var users = dbContext.Users
                    .Where(u => u.Id == userId || u.Id == requestedUser)
                    .ToListAsync().Result;
                return (Ok(new ChatResponse{Users = users, ChatId = existingChat.Result.ChatId }));*/
            }
            else
            {
                var users = dbContext.Users
                    .Where(u => u.Id == userId || u.Id == requestedUser)
                    .ToListAsync().Result;
                var newChat = new Chat(users[0].Id, users[1].Id);
                dbContext.Chats.AddAsync(newChat);
                dbContext.SaveChanges();
                _krolikMq.CreateQueue(newChat.ChatId);
                return (Ok(new ChatResponse{Users = users, ChatId = newChat.ChatId}));
            }
        }
        if(!chatId.ToString().IsNullOrEmpty())
        {
            var existingChat =  dbContext.Chats
                .FirstOrDefaultAsync(c => c.ChatId == chatId.Value &&c.Users.Contains(requestedUser));
            if (existingChat.Result !=default)
            {
                var users = dbContext.Users
                    .Where(u => u.Id == existingChat
                        .Result.Users
                        .FirstOrDefault(id=>id!=requestedUser) || u.Id == requestedUser)
                    .ToListAsync().Result;
                var messages = dbContext
                    .Messages
                    .Where(m => m.ChatId == existingChat.Result.ChatId)
                    .ToListAsync().Result;
                var messagesResponse = new List<MessagesResponse>() { };
                messages.ForEach(m=>messagesResponse
                    .Add(new MessagesResponse
                    {
                        Text = m.Text, 
                        UserId = m.UserId, 
                        DateTime = m.DateTime
                    }));
                return (Ok(new ChatResponse{Users = users, ChatId = existingChat.Result.ChatId, Messages = messagesResponse}));
            }
        }
        return (Problem());
    }
    
    
}