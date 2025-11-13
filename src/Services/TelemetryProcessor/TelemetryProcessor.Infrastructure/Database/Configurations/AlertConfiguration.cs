using IoTTelemetry.Domain.Entities;
using IoTTelemetry.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TelemetryProcessor.Infrastructure.Database.Converters;

namespace TelemetryProcessor.Infrastructure.Database.Configurations;

/// <summary>
/// EF Core entity configuration for Alert entity.
/// </summary>
public sealed class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder.ToTable("alerts");

        // Primary key
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasColumnName("alert_id")
            .IsRequired();

        // Device reference
        builder.Property(a => a.DeviceId)
            .HasConversion(new DeviceIdConverter())
            .HasColumnName("device_id")
            .IsRequired();

        // Properties
        builder.Property(a => a.Severity)
            .HasColumnName("severity")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.Message)
            .HasColumnName("message")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(a => a.IsAcknowledged)
            .HasColumnName("is_acknowledged")
            .IsRequired();

        builder.Property(a => a.AcknowledgedBy)
            .HasColumnName("acknowledged_by")
            .HasMaxLength(200);

        builder.Property(a => a.Resolution)
            .HasColumnName("resolution")
            .HasMaxLength(2000);

        // Timestamps
        builder.Property(a => a.Timestamp)
            .HasConversion(new TimestampConverter())
            .HasColumnName("timestamp")
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .HasConversion(new TimestampConverter())
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(a => a.AcknowledgedAt)
            .HasConversion(new NullableTimestampConverter())
            .HasColumnName("acknowledged_at");

        // Metadata dictionary as JSON
        builder.Property(a => a.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb")
            .IsRequired();

        // Indexes
        builder.HasIndex(a => a.DeviceId)
            .HasDatabaseName("ix_alerts_device_id");

        builder.HasIndex(a => a.Severity)
            .HasDatabaseName("ix_alerts_severity");

        builder.HasIndex(a => a.Timestamp)
            .HasDatabaseName("ix_alerts_timestamp");

        builder.HasIndex(a => a.IsAcknowledged)
            .HasDatabaseName("ix_alerts_is_acknowledged");

        builder.HasIndex(a => new { a.DeviceId, a.Timestamp })
            .HasDatabaseName("ix_alerts_device_timestamp");
    }
}
