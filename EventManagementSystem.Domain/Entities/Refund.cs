using EventManagementSystem.Domain.Events;
using EventManagementSystem.Domain.ValueObjects;


namespace EventManagementSystem.Domain.Entities;

public class Refund
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public Guid Id { get; private set; }
    public Guid BookingId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Money Amount { get; private set; }
    public RefundStatus Status { get; private set; }
    public string Reason { get; private set; }

    private Refund() { Reason = null!; Amount = null!; }
    public string? RejectionReason { get; private set; }
    public string? PaymentReference { get; private set; }

    public Refund(Booking booking, Event @event, string reason)
    {
        if (booking.Status != BookingStatus.Paid)
            throw new InvalidOperationException("Refund can only be requested for paid bookings.");

        if (booking.Tickets.Any(t => t.Status == TicketStatus.CheckedIn))
            throw new InvalidOperationException("Cannot request refund. One or more tickets have been checked in.");

        if (@event.Status != EventStatus.Cancelled)
        {
            if (DateTime.UtcNow > @event.StartDate)
                throw new InvalidOperationException("The refund deadline has passed.");
        }

        Id = Guid.NewGuid();
        BookingId = booking.Id;
        CustomerId = booking.CustomerId;
        Amount = booking.TotalPrice;

        Status = RefundStatus.Requested;

        AddDomainEvent(new RefundRequested(Id, BookingId, CustomerId));
    }

    public void Approve()
    {
        if (Status != RefundStatus.Requested)
            throw new InvalidOperationException("Only requested refunds can be approved.");

        Status = RefundStatus.Approved;

        AddDomainEvent(new RefundApproved(Id, BookingId));
    }

    public void Reject(string rejectionReason)
    {
        if (Status != RefundStatus.Requested)
            throw new InvalidOperationException("Only requested refunds can be rejected.");

        if (string.IsNullOrWhiteSpace(rejectionReason))
            throw new ArgumentException("A rejection reason must be provided.");

        Status = RefundStatus.Rejected;
        RejectionReason = rejectionReason;

        AddDomainEvent(new RefundRejected(Id, RejectionReason));
    }

    public void MarkAsPaidOut(string paymentReference)
    {
        if (Status != RefundStatus.Approved)
            throw new InvalidOperationException("Only approved refunds can be marked as paid out.");

        if (string.IsNullOrWhiteSpace(paymentReference))
            throw new ArgumentException("A payment reference must be provided.");

        Status = RefundStatus.PaidOut;
        PaymentReference = paymentReference;

        AddDomainEvent(new RefundPaidOut(Id, PaymentReference));
    }
}