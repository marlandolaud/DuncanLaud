using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DuncanLaud.Analytics;

/// <summary>
/// Allows dotnet-ef to create the AnalyticsDbContext at design time without a running app.
/// </summary>
public class AnalyticsDbContextFactory : IDesignTimeDbContextFactory<AnalyticsDbContext>
{
    public AnalyticsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AnalyticsDbContext>()
            .UseSqlServer("Server=localhost,1433;Database=duncanlaud_analytics;User Id=sa;Password=YourLocalPassword!1;TrustServerCertificate=true;")
            .Options;

        return new AnalyticsDbContext(options);
    }
}
