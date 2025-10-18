namespace IoTTelemetry.Domain.Common;

/// <summary>
/// Base class for all entities in the domain.
/// Entities have identity and lifecycle.
/// </summary>
#pragma warning disable S4035 // Base class designed for inheritance
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
#pragma warning restore S4035
{
    protected Entity(TId id)
    {
        Id = id;
    }

    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    public TId Id { get; protected init; }

    public override bool Equals(object? obj)
    {
        return obj is Entity<TId> entity && Id.Equals(entity.Id);
    }

    public bool Equals(Entity<TId>? other)
    {
        return Equals((object?)other);
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !Equals(left, right);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
