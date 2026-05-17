namespace EventManagementSystem.Domain.Events;

// Ini adalah bentuk Domain Event yang benar
public record RefundApproved(Guid RefundId, Guid BookingId) : IDomainEvent;