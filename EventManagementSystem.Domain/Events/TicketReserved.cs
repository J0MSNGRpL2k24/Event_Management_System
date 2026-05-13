namespace EventManagementSystem.Domain.Events;

public record TicketReserved(Guid BookingId, Guid EventId, Guid CustomerId) : IDomainEvent;