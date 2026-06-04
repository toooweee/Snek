using Microsoft.EntityFrameworkCore;
using Snek.Models;

namespace Snek.Data;

public class AppDbContext : DbContext
{
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!; 
    public DbSet<Manufacturer> Manufacturers { get; set; } = null!; 
    public DbSet<Supplier> Suppliers { get; set; } = null!; 
    public DbSet<UnitMeasure> UnitMeasures { get; set; } = null!; 
    public DbSet<Product> Products { get; set; } = null!; 
    public DbSet<PickupPoint> PickupPoints => Set<PickupPoint>();
    public DbSet<OrderStatus> OrderStatuses => Set<OrderStatus>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseLazyLoadingProxies()
            .UseNpgsql("Host=localhost;port=5454;Username=postgres;password=1;Database=snek");
    }
}