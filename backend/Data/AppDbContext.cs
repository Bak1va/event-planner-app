using Backend.Entities;
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
    public DbSet<UserEvent> UserEvents => Set<UserEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("ix_users_email");

            entity.Property(u => u.DateAdded)
                .HasDefaultValueSql("now()")
                .ValueGeneratedOnAdd();

            entity.Property(u => u.DateModified)
                .HasDefaultValueSql("now()")
                .ValueGeneratedOnAddOrUpdate();
        });

        modelBuilder.Entity<EventItem>(entity =>
        {
            entity.HasOne(e => e.User)
                .WithMany(u => u.OrganizedEvents)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.DateAdded)
                .HasDefaultValueSql("now()")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.DateModified)
                .HasDefaultValueSql("now()")
                .ValueGeneratedOnAddOrUpdate();
        });

        modelBuilder.Entity<UserEvent>(entity =>
        {
            entity.HasOne(ue => ue.User)
                .WithMany(u => u.AttendingEvents)
                .HasForeignKey(ue => ue.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ue => ue.Event)
                .WithMany(e => e.Attendees)
                .HasForeignKey(ue => ue.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(ue => new { ue.UserId, ue.EventId })
                .IsUnique()
                .HasDatabaseName("ix_user_events_unique");

            entity.Property(ue => ue.DateJoined)
                .HasDefaultValueSql("now()")
                .ValueGeneratedOnAdd();
        });

        base.OnModelCreating(modelBuilder);
    }
}
