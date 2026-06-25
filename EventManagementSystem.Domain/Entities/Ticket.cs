using EventManagementSystem.Domain.Events;

namespace EventManagementSystem.Domain.Entities;

public enum TicketStatus
{
    Active,
    CheckedIn,
    Cancelled
}

public class Ticket
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public Guid Id { get; private set; }
    public Guid BookingId { get; private set; }
    public Guid EventId { get; private set; }
    public string TicketCode { get; private set; }
    public TicketStatus Status { get; private set; }

    private Ticket() { TicketCode = null!; }

    internal Ticket(Guid bookingId, Guid eventId, string ticketCode)
    {
        Id = Guid.NewGuid();
        BookingId = bookingId;
        EventId = eventId;
        TicketCode = ticketCode;
        Status = TicketStatus.Active;
    }

    public void CheckIn(Event @event)
    {
        if (@event.Status == EventStatus.Cancelled)
            throw new InvalidOperationException("The event has been cancelled.");

        if (@event.Id != EventId)
            throw new InvalidOperationException("The ticket does not match the event.");

        if (Status == TicketStatus.CheckedIn)
            throw new InvalidOperationException("The ticket has already been used.");

        if (Status != TicketStatus.Active)
            throw new InvalidOperationException("Ticket is not active.");

        if (DateTime.UtcNow.Date != @event.StartDate.Date)
            throw new InvalidOperationException("Check-in can only be performed on the event day.");

        
        Status = TicketStatus.CheckedIn;
        AddDomainEvent(new TicketCheckedIn(Id, EventId, TicketCode));
    }


    
    internal void Cancel() 
    {
        if (Status == TicketStatus.CheckedIn)
            throw new InvalidOperationException("Cannot cancel a checked-in ticket.");

        Status = TicketStatus.Cancelled;
    }
}