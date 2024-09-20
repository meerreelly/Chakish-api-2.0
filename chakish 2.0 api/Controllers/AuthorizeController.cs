using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using chakish_2._0_api.Data_Base;
using chakish_2._0_api.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace chakish_2._0_api.Controllers;

[ApiController]
public class AuthorizeController(DataService dbContext, IConfiguration configuration) : ControllerBase
{
    private readonly DataService _dbContext = dbContext;
    private readonly IConfiguration _configuration = configuration;
    
    [HttpGet("/login-google")]
    public IActionResult GoogleLogin()
    {
        var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("/google-response")]
    public async Task<IActionResult> GoogleResponse()
    {
        var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
    
        if (!authenticateResult.Succeeded)
        {
            return Unauthorized();
        }

        // Отримання інформації про користувача з Google
        var claims = authenticateResult.Principal?.Identities.FirstOrDefault()?.Claims;
        var googleId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

        if (googleId == null || email == null || name == null)
        {
            return Problem("Failed to retrieve Google user information.");
        }

        // Перевірка наявності користувача в базі даних
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);

        if (user == null)
        {
            // Якщо користувача немає в базі, додаємо нового
            user = new User(googleId, email, name);
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
        }

        // Генерація JWT токена для цього користувача
        var token = GenerateJwtToken(user);


        return Redirect($"https://chakishfront.serveo.net/google-callback?token={Uri.EscapeDataString(token)}");
    }


    private string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.GoogleId),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("id", user.Id.ToString()), 
            new Claim(ClaimTypes.Role, user.Role) // Зберігаємо роль користувача
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(60),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
