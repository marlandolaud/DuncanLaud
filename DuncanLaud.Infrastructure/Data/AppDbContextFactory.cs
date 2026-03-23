using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DuncanLaud.Infrastructure.Data;

/// <summary>
/// Allows dotnet-ef to create the DbContext at design time without a running app.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=localhost,1433;Database=duncanlaud;User Id=sa;Password=YourLocalPassword!1;TrustServerCertificate=true;")
            .Options;

        return new AppDbContext(options);
    }
}
