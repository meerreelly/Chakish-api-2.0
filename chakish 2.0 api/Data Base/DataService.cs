using Microsoft.Extensions.Configuration;

namespace chakish_2._0_api.Data_Base;
using Microsoft.EntityFrameworkCore;
using chakish_2._0_api.Models;

public class DataService(IConfiguration configuration): DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Chat> Chats => Set<Chat>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("Database"));
    }
}