namespace EventManagementSystem.Domain.Events;

public record RefundRejected(Guid RefundId, string RejectionReason) : IDomainEvent;