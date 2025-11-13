using IoTTelemetry.Domain.Entities;
using IoTTelemetry.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TelemetryProcessor.Infrastructure.Database.Converters;

namespace TelemetryProcessor.Infrastructure.Database.Configurations;

/// <summary>
/// EF Core entity configuration for Device aggregate.
/// </summary>
public sealed class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("devices");

        // Primary key
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id)
            .HasConversion(new DeviceIdConverter())
            .HasColumnName("device_id")
            .IsRequired();

        // Properties
        builder.Property(d => d.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(d => d.Type)
            .HasColumnName("type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(d => d.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(d => d.Location)
            .HasColumnName("location")
            .HasMaxLength(500);

        // Timestamps
        builder.Property(d => d.CreatedAt)
            .HasConversion(new TimestampConverter())
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(d => d.LastSeenAt)
            .HasConversion(new NullableTimestampConverter())
            .HasColumnName("last_seen_at");

        builder.Property(d => d.LastModifiedAt)
            .HasConversion(new NullableTimestampConverter())
            .HasColumnName("last_modified_at");

        // Properties dictionary as JSON
        builder.Property(d => d.Properties)
            .HasColumnName("properties")
            .HasColumnType("jsonb")
            .IsRequired();

        // Indexes
        builder.HasIndex(d => d.Name)
            .HasDatabaseName("ix_devices_name");

        builder.HasIndex(d => d.Type)
            .HasDatabaseName("ix_devices_type");

        builder.HasIndex(d => d.Status)
            .HasDatabaseName("ix_devices_status");

        builder.HasIndex(d => d.LastSeenAt)
            .HasDatabaseName("ix_devices_last_seen_at");

        // Ignore domain events (not persisted)
        builder.Ignore(d => d.DomainEvents);
    }
}
