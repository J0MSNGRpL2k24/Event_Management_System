using EventManagementSystem.Domain.Events;
using EventManagementSystem.Domain.ValueObjects;

namespace EventManagementSystem.Domain.Entities;

public class Booking
{
    // 1. Properties
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid EventId { get; private set; }
    public Guid CategoryId { get; private set; }
    public int Quantity { get; private set; }
    public Money TotalPrice { get; private set; }
    public BookingStatus Status { get; private set; }
    public DateTime PaymentDeadline { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Booking() { }

   
    public static Booking Create(
        Guid customerId,
        Event @event,
        TicketCategory category,
        int quantity,
        decimal serviceFee = 5000)
    {
       
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");

        
        var subtotal = category.Price.Amount * quantity;
        var totalWithFee = subtotal + serviceFee;

        
        var totalPrice = new Money(totalWithFee, category.Price.Currency);

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            EventId = @event.Id,
            CategoryId = category.Id,
            Quantity = quantity,
            TotalPrice = totalPrice,
            Status = BookingStatus.PendingPayment,
            PaymentDeadline = DateTime.UtcNow.AddMinutes(15),
            CreatedAt = DateTime.UtcNow
        };

       
        booking.AddDomainEvent(new TicketReserved(booking.Id, @event.Id, customerId));

        return booking;
    }

    
    public void ConfirmPayment()
    {
        if (Status != BookingStatus.PendingPayment)
            throw new InvalidOperationException("Booking is not in a pending state.");

        if (DateTime.UtcNow > PaymentDeadline)
        {
            Expire();
            throw new InvalidOperationException("Payment deadline has passed.");
        }

        Status = BookingStatus.Paid;
        AddDomainEvent(new BookingPaid(Id, CustomerId));
    }

   
    public void Expire()
    {
        if (Status == BookingStatus.PendingPayment)
        {
            Status = BookingStatus.Expired;
            AddDomainEvent(new BookingExpired(Id, EventId, CategoryId, Quantity));
        }
    }

    private void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}