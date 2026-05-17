using Microsoft.EntityFrameworkCore;
using ReviewerPortal.API.Domain.Entities;

namespace ReviewerPortal.API.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<University> Universities { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<University>()
            .HasIndex(u => u.UniversityName)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.UserName)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasOne(u => u.University)
            .WithMany()
            .HasForeignKey(u => u.UniversityId);
    }
}
