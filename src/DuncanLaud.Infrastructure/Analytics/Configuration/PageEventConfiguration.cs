using DuncanLaud.Analytics.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DuncanLaud.Analytics.Configuration;

public class PageEventConfiguration : IEntityTypeConfiguration<PageEvent>
{
    public void Configure(EntityTypeBuilder<PageEvent> builder)
    {
        builder.ToTable("page_events");
        builder.HasKey(e => e.PageEventId);
        builder.Property(e => e.PageEventId).UseIdentityColumn();
        builder.Property(e => e.UrlPath).HasMaxLength(1000).IsRequired();
        builder.Property(e => e.UrlQuery).HasMaxLength(2000);
        builder.Property(e => e.Referrer).HasMaxLength(2000);
        builder.Property(e => e.HttpMethod).HasMaxLength(10).HasDefaultValue("GET").IsRequired();
        builder.Property(e => e.StatusCode).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnType("datetime2(3)").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.HasOne(e => e.Session).WithMany(s => s.PageEvents).HasForeignKey(e => e.SessionId);
        builder.HasIndex(e => new { e.SessionId, e.CreatedAt }).HasDatabaseName("IX_page_events_session");
        builder.HasIndex(e => new { e.UrlPath, e.CreatedAt }).HasDatabaseName("IX_page_events_path");
    }
}
