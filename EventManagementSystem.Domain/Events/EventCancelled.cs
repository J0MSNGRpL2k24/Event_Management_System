namespace EventManagementSystem.Domain.Events;

public record EventCancelled(Guid EventId) : IDomainEvent;