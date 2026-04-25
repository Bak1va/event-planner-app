using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<EventItem> Events => Set<EventItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Name).IsRequired().HasMaxLength(100);
            entity.Property(user => user.Email).IsRequired().HasMaxLength(150);
            entity.Property(user => user.PasswordHash).IsRequired();
        });

        modelBuilder.Entity<EventItem>(entity =>
        {
            entity.HasKey(eventItem => eventItem.Id);
            entity.Property(eventItem => eventItem.Name).IsRequired().HasMaxLength(150);
            entity.Property(eventItem => eventItem.Status).IsRequired().HasMaxLength(100);
            entity.Property(eventItem => eventItem.Description).HasMaxLength(1000);
        });
    }
}
