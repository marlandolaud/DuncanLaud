using DuncanLaud.Analytics.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DuncanLaud.Analytics.Configuration;

public class ApiEventConfiguration : IEntityTypeConfiguration<ApiEvent>
{
    public void Configure(EntityTypeBuilder<ApiEvent> builder)
    {
        builder.ToTable("api_events");
        builder.HasKey(e => e.ApiEventId);
        builder.Property(e => e.ApiEventId).UseIdentityColumn();
        builder.Property(e => e.Endpoint).HasMaxLength(1000).IsRequired();
        builder.Property(e => e.HttpMethod).HasMaxLength(10).IsRequired();
        builder.Property(e => e.StatusCode).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnType("datetime2(3)").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.HasOne(e => e.Session).WithMany(s => s.ApiEvents).HasForeignKey(e => e.SessionId);
        builder.HasIndex(e => new { e.SessionId, e.CreatedAt }).HasDatabaseName("IX_api_events_session");
        builder.HasIndex(e => new { e.Endpoint, e.CreatedAt }).HasDatabaseName("IX_api_events_endpoint");
    }
}
