namespace SSD.Domain.Common;

public abstract class AggregateRoot : AuditableEntity
{
    protected AggregateRoot()
    {
    }

    protected AggregateRoot(Guid id, DateTimeOffset? createdUtc = null)
        : base(id, createdUtc)
    {
    }
}
