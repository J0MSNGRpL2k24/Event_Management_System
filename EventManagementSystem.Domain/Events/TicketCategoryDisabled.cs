namespace EventManagementSystem.Domain.Events;

public record TicketCategoryDisabled(Guid EventId, Guid CategoryId) : IDomainEvent;