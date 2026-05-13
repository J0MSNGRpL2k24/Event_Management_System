namespace EventManagementSystem.Domain.Events;

public record BookingPaid(
    Guid BookingId,
    Guid CustomerId
) : IDomainEvent;