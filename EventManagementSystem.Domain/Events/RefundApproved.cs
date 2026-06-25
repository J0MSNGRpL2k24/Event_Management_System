namespace EventManagementSystem.Domain.Events;

public record RefundApproved(Guid RefundId, Guid BookingId) : IDomainEvent;