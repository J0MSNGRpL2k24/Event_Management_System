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

    private Refund() { Reason = null!; Amount = null!; } // for EF Core
    public string? RejectionReason { get; private set; }
    public string? PaymentReference { get; private set; }

    public Refund(Booking booking, Event @event, string reason)
    {
        // AC 1: A refund can only be requested for a booking with the status Paid.
        if (booking.Status != BookingStatus.Paid)
            throw new InvalidOperationException("Refund can only be requested for paid bookings.");

        // AC 2: A refund cannot be requested if any ticket from the booking has already been checked in.
        if (booking.Tickets.Any(t => t.Status == TicketStatus.CheckedIn))
            throw new InvalidOperationException("Cannot request refund. One or more tickets have been checked in.");

        // AC 4: If the event is cancelled, a refund is automatically allowed. 
        // AC 3: A refund can only be requested before the refund deadline.
        // (Jika event batal, abaikan deadline. Jika tidak batal,cek deadline/tanggal mulai).
        if (@event.Status != EventStatus.Cancelled)
        {
            // if the event is not cancelled, check if the current date is before the event start date (refund deadline)
            if (DateTime.UtcNow > @event.StartDate)
                throw new InvalidOperationException("The refund deadline has passed.");
        }

        Id = Guid.NewGuid();
        BookingId = booking.Id;
        CustomerId = booking.CustomerId;
        Amount = booking.TotalPrice; // Uang yang dikembalikan = total bayar
        Reason = reason;

        // AC 5: A refund must have one of the following statuses (Default is Requested)
        Status = RefundStatus.Requested;

        // AC 6: System raises the domain event RefundRequested
        AddDomainEvent(new RefundRequested(Id, BookingId, CustomerId));
    }

    public void Approve()
    {
        // AC: A refund can only be approved if its status is Requested.
        if (Status != RefundStatus.Requested)
            throw new InvalidOperationException("Only requested refunds can be approved.");

        // AC: When a refund is approved, its status changes to Approved.
        Status = RefundStatus.Approved;

        // AC: After a refund is approved, the system raises the domain event RefundApproved.
        AddDomainEvent(new RefundApproved(Id, BookingId));
    }

    public void Reject(string rejectionReason)
    {
        // AC 1: A refund can only be rejected if its status is Requested.
        if (Status != RefundStatus.Requested)
            throw new InvalidOperationException("Only requested refunds can be rejected.");

        // AC 2: A rejection reason must be provided.
        if (string.IsNullOrWhiteSpace(rejectionReason))
            throw new ArgumentException("A rejection reason must be provided.");

        // AC 3: When a refund is rejected, its status changes to Rejected.
        Status = RefundStatus.Rejected;
        RejectionReason = rejectionReason;

        // AC 6: After a refund is rejected, the system raises the domain event.
        AddDomainEvent(new RefundRejected(Id, RejectionReason));
    }

    public void MarkAsPaidOut(string paymentReference)
    {
        // AC 1: A refund can only be marked as paid out if its status is Approved.
        // AC 4: A paid-out refund cannot be approved, rejected, or cancelled again (ter-cover oleh logika ini).
        if (Status != RefundStatus.Approved)
            throw new InvalidOperationException("Only approved refunds can be marked as paid out.");

        // AC 2: A payment reference must be recorded.
        if (string.IsNullOrWhiteSpace(paymentReference))
            throw new ArgumentException("A payment reference must be provided.");

        // AC 3: When the refund is paid out, its status changes to PaidOut.
        Status = RefundStatus.PaidOut;
        PaymentReference = paymentReference;

        // AC 5: After the refund is paid out, the system raises the domain event RefundPaidOut.
        AddDomainEvent(new RefundPaidOut(Id, PaymentReference));
    }
}