namespace EventManagementSystem.Domain.Events;

public record RefundRequested(Guid RefundId, Guid BookingId, Guid CustomerId) : IDomainEvent;