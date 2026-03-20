using SSD.Domain.Common;
using SSD.Domain.Enums;

namespace SSD.Domain.Entities;

public sealed class AuditLog : AuditableEntity
{
    public AuditLog()
    {
        ActorType = AuditActorType.System;
        Action = string.Empty;
        EntityName = string.Empty;
        EntityId = string.Empty;
        MetadataJson = "{}";
    }

    public AuditLog(Guid id, AuditActorType actorType, string action, string entityName, string entityId)
        : base(id)
    {
        ActorType = actorType;
        Action = action;
        EntityName = entityName;
        EntityId = entityId;
        MetadataJson = "{}";
    }

    public Guid? UserId { get; private set; }

    public AuditActorType ActorType { get; private set; }

    public string Action { get; private set; }

    public string EntityName { get; private set; }

    public string EntityId { get; private set; }

    public string? CorrelationId { get; private set; }

    public string? IpAddress { get; private set; }

    public string MetadataJson { get; private set; }

    public User? User { get; private set; }
}
