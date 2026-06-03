using Microsoft.EntityFrameworkCore;
using Snek.Models;

namespace Snek.Data;

public class AppDbContext : DbContext
{
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseLazyLoadingProxies()
            .UseNpgsql("Host=localhost;port=5454;Username=postgres;password=1;Database=snek");
    }
}