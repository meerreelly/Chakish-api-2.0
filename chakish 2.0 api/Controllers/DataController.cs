using System.Security.Claims;
using chakish_2._0_api.Data_Base;
using chakish_2._0_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace chakish_2._0_api.Controllers;

[ApiController]
public class DataController(DataService dbContext, IConfiguration configuration) : ControllerBase
{
    private readonly DataService _dbContext = dbContext;
    private readonly IConfiguration _configuration = configuration;
    
    [Authorize]
    [HttpGet("/user-info")]
    public IActionResult GetUserInfo()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = _dbContext.Users.FirstOrDefault(u => u.GoogleId == userId);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }
    [Authorize]
    [HttpPost("/set-avatar")]
    public IActionResult SetAvatar([FromBody] string filePath)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = _dbContext.Users.FirstOrDefault(u => u.GoogleId == userId);
            if (user == null)
            {
                return NotFound();
            }

            user.AvatarUrl = filePath;
            dbContext.SaveChanges();
            return Ok(user);
        }
        catch
        {
            return Problem();
        }
    }

    [Authorize]
    [HttpGet("/get-user-info")]
    public IActionResult GetUserInfoDto([FromQuery] Guid userId)
    {
        var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(new UserDto{Name = user.Name,AvatarUrl = user.AvatarUrl, Id = userId, Role = user.Role});
    }
}