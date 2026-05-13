namespace EventManagementSystem.Domain.Events;

public record TicketCategoryCreated(Guid EventId, Guid CategoryId, string Name) : IDomainEvent;