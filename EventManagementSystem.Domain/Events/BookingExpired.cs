namespace EventManagementSystem.Domain.Events;

public record BookingExpired(
    Guid BookingId,
    Guid EventId,
    Guid CategoryId,
    int Quantity
) : IDomainEvent;