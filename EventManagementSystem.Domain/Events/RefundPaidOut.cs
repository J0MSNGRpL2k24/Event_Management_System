namespace EventManagementSystem.Domain.Events;

public record RefundPaidOut(Guid RefundId, string PaymentReference) : IDomainEvent;