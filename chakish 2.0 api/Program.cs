using System.Text;
using chakish_2._0_api;
using chakish_2._0_api.Data_Base;
using chakish_2._0_api.SignalRConroller;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Configure Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme; // Specify default sign-in scheme
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception is SecurityTokenExpiredException)
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/login-google";
})
.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.CallbackPath = "/signin-google";
    options.SaveTokens = true;
});


// Add Authorization
builder.Services.AddAuthorization();

// Add Controllers and SignalR
builder.Services.AddControllers();
builder.Services.AddSignalR(); // Add SignalR

// Add Singleton for RabbitMQ service (or KrolikMQ)
builder.Services.AddSingleton<KrolikMQ>();

// Configure Swagger (for API documentation)
builder.Services.AddSwaggerGen();

// Dependency Injection for DataService
builder.Services.AddScoped<DataService>();

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(
            "https://chakishfront.serveo.net",
            "https://chakishbackend.serveo.net",
            "http://localhost:5095") // Allow specified domains
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Allow cookies and tokens
    });
});

var app = builder.Build();

// Enable HTTPS Redirection
app.UseHttpsRedirection();

// Enable Routing
app.UseRouting();

// Enable Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

// Enable CORS (Cross-Origin Resource Sharing)
app.UseCors("AllowSpecificOrigins");

// Configure Endpoints
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<ChatHub>("/chathub"); // Map SignalR hubs
});

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DataService>();
    await dbContext.Database.EnsureCreatedAsync();
}

// Configure Swagger for Development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Run the application
app.Run();
