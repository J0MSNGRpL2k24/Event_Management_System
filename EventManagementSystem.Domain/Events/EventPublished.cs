namespace EventManagementSystem.Domain.Events;

public record EventPublished(Guid EventId) : IDomainEvent;