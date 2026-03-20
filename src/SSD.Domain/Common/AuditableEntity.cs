namespace SSD.Domain.Common;

public abstract class AuditableEntity : Entity
{
    protected AuditableEntity()
    {
    }

    protected AuditableEntity(Guid id, DateTimeOffset? createdUtc = null)
        : base(id)
    {
        var now = createdUtc ?? DateTimeOffset.UtcNow;
        CreatedUtc = now;
        UpdatedUtc = now;
    }

    public DateTimeOffset CreatedUtc { get; protected set; }

    public DateTimeOffset UpdatedUtc { get; protected set; }

    public void Touch(DateTimeOffset? updatedUtc = null)
    {
        UpdatedUtc = updatedUtc ?? DateTimeOffset.UtcNow;
    }
}
