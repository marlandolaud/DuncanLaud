using DuncanLaud.Analytics.Configuration;
using DuncanLaud.Analytics.Entities;
using Microsoft.EntityFrameworkCore;

namespace DuncanLaud.Analytics;

public class AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : DbContext(options)
{
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<PageEvent> PageEvents => Set<PageEvent>();
    public DbSet<ApiEvent> ApiEvents => Set<ApiEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("analytics");
        modelBuilder.ApplyConfiguration(new SessionConfiguration());
        modelBuilder.ApplyConfiguration(new PageEventConfiguration());
        modelBuilder.ApplyConfiguration(new ApiEventConfiguration());
    }
}
