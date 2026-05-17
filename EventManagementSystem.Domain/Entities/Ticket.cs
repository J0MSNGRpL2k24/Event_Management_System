using EventManagementSystem.Domain.Events; // Wajib ada agar TicketCheckedIn terbaca

namespace EventManagementSystem.Domain.Entities;

public enum TicketStatus
{
    Active,
    CheckedIn,
    Cancelled
}

public class Ticket // HAPUS tulisan ": Entity" di sini
{
    // 1. Tambahkan penampung Domain Event secara manual
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    // 2. Properti inti tiket
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

    // 3. Logika Check-In (US-13)
    public void CheckIn(Event @event)
    {
        // AC 4: If the event has been cancelled...
        if (@event.Status == EventStatus.Cancelled)
            throw new InvalidOperationException("The event has been cancelled.");

        // AC 3: If the ticket belongs to a different event...
        if (@event.Id != EventId)
            throw new InvalidOperationException("The ticket does not match the event.");

        // AC 2: If the ticket has already been checked in...
        if (Status == TicketStatus.CheckedIn)
            throw new InvalidOperationException("The ticket has already been used.");

        if (Status != TicketStatus.Active)
            throw new InvalidOperationException("Ticket is not active.");

        // Validasi tambahan (dari US-13)
        if (DateTime.UtcNow.Date != @event.StartDate.Date)
            throw new InvalidOperationException("Check-in can only be performed on the event day.");

        // AC 5: The ticket status must not change if check-in fails.
        // (Karena pengecekan di atas menggunakan 'throw', baris di bawah ini tidak akan pernah tereksekusi jika tiketnya invalid/gagal)

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