using DuncanLaud.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace DuncanLaud.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Group> Groups => Set<Group>();
    public DbSet<Person> Persons => Set<Person>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Group>(e =>
        {
            e.HasKey(g => g.Id);
            e.Property(g => g.Id).ValueGeneratedNever();
            e.Property(g => g.Name).IsRequired().HasMaxLength(100);
            e.Property(g => g.CreatedAtUtc).IsRequired();
        });

        modelBuilder.Entity<Person>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).ValueGeneratedNever();

            e.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
            e.Property(p => p.LastName).IsRequired().HasMaxLength(100);
            e.Property(p => p.PreferredName).HasMaxLength(100);
            e.Property(p => p.ImageContentType).HasMaxLength(50);
            e.Property(p => p.BirthDate).HasColumnType("date");
            e.Property(p => p.CreatedAtUtc).IsRequired();

            e.HasOne(p => p.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(p => p.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(p => p.GroupId)
                .HasDatabaseName("IX_Person_GroupId");

            e.HasIndex(p => new { p.GroupId, p.BirthDate })
                .HasDatabaseName("IX_Person_GroupId_BirthDate");
        });
    }
}
