using DuncanLaud.Analytics.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DuncanLaud.Analytics.Configuration;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("sessions");
        builder.HasKey(s => s.SessionId);
        builder.Property(s => s.SessionId).UseIdentityColumn();
        builder.Property(s => s.IpHash).HasMaxLength(64).IsRequired();
        builder.Property(s => s.IpPrefix).HasMaxLength(16).IsRequired();
        builder.Property(s => s.UserAgentHash).HasMaxLength(64).IsRequired();
        builder.Property(s => s.UserAgentRaw).HasMaxLength(512);
        builder.Property(s => s.SessionStart).HasColumnType("datetime2(3)").IsRequired();
        builder.Property(s => s.SessionEnd).HasColumnType("datetime2(3)");
        builder.Property(s => s.EventCount).HasDefaultValue(0).IsRequired();
        builder.Property(s => s.IsBot).HasDefaultValue(false).IsRequired();
        builder.HasIndex(s => new { s.IpHash, s.SessionStart }).HasDatabaseName("IX_sessions_ip_hash");
    }
}
