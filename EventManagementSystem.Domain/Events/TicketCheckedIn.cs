namespace EventManagementSystem.Domain.Events;

public record TicketCheckedIn(Guid TicketId, Guid EventId, string TicketCode) : IDomainEvent;