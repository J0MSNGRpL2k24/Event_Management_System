using EventManagementSystem.Domain.Events;
using EventManagementSystem.Domain.ValueObjects;

namespace EventManagementSystem.Domain.Entities;

public class Event
{
    public Guid Id { get; private set; }
    public Guid OrganizerId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public string Location { get; private set; }
    public int MaxCapacity { get; private set; }
    public EventStatus Status { get; private set; }
    private readonly List<TicketCategory> _categories = new();
    public IReadOnlyCollection<TicketCategory> Categories => _categories.AsReadOnly();

    
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // Private constructor untuk enkapsulasi
    private Event() { }

    public static Event Create(
        Guid organizerId,
        string name,
        string description,
        DateTime startDate,
        DateTime endDate,
        string location,
        int maxCapacity)
    {
        // Business Rule: Max capacity must be > 0
        if (maxCapacity <= 0)
            throw new ArgumentException("Maximum capacity must be greater than zero.");

        // Business Rule: End date cannot be earlier than start date
        if (endDate < startDate)
            throw new ArgumentException("End date cannot be earlier than the start date.");

        var @event = new Event
        {
            Id = Guid.NewGuid(),
            OrganizerId = organizerId,
            Name = name,
            Description = description,
            StartDate = startDate,
            EndDate = endDate,
            Location = location,
            MaxCapacity = maxCapacity,
            Status = EventStatus.Draft // Rule: Newly created event must be Draft
        };

        // Rule: Raise EventCreated domain event
        @event._domainEvents.Add(new EventCreated(@event.Id, @event.Name));

        return @event; }

        public void AddTicketCategory(string name, Money price, int quota, DateTime salesStart, DateTime salesEnd)
    {
        // AC: Quota must be > 0
        if (quota <= 0)
            throw new ArgumentException("Ticket quota must be greater than zero.");

        // AC: Total quota cannot exceed max event capacity
        int currentTotalQuota = _categories.Sum(c => c.Quota);
        if (currentTotalQuota + quota > MaxCapacity)
            throw new InvalidOperationException("Total quota of all categories exceeds event capacity.");

        // AC: Sales period must end before or at event start date
        if (salesEnd > StartDate)
            throw new ArgumentException("Ticket sales period must end before the event starts.");

        var category = new TicketCategory(Id, name, price, quota, salesStart, salesEnd);
        _categories.Add(category);

        // AC: Raise domain event
        _domainEvents.Add(new TicketCategoryCreated(Id, category.Id, name));
    }


    public void Publish()
    {
        // AC: An event with the status Cancelled cannot be published
        if (Status == EventStatus.Cancelled)
            throw new InvalidOperationException("A cancelled event cannot be published.");

        // AC: An event with the status Draft can be changed to Published
        if (Status != EventStatus.Draft)
            throw new InvalidOperationException("Only draft events can be published.");

        // AC: Must have at least one active ticket category
        if (!_categories.Any(c => c.IsActive))
            throw new InvalidOperationException("Event must have at least one active ticket category to be published.");

        // AC: Total ticket quota does not exceed the maximum event capacity
        // (Sebenarnya sudah dijaga di AddTicketCategory, tapi ini sebagai double-check/invariant akhir)
        int totalQuota = _categories.Sum(c => c.Quota);
        if (totalQuota > MaxCapacity)
            throw new InvalidOperationException("Total ticket quota exceeds event capacity.");

        Status = EventStatus.Published;

        // AC: Raise domain event EventPublished
        _domainEvents.Add(new EventPublished(Id));
    }

    public void ReserveTickets(Guid categoryId, int quantity)
    {
        // AC: Event must be Published
        if (Status != EventStatus.Published)
            throw new InvalidOperationException("Cannot reserve tickets for an unpublished event.");

        var category = _categories.FirstOrDefault(c => c.Id == categoryId);

        // AC: Category must exist and be active
        if (category == null || !category.IsActive)
            throw new InvalidOperationException("Ticket category is not available.");

        // AC: Within sales period
        var now = DateTime.UtcNow;
        if (now < category.SalesStart || now > category.SalesEnd)
            throw new InvalidOperationException("Ticket is outside of sales period.");

        // Eksekusi pengurangan kuota di level category
        category.ReserveTickets(quantity);
    }

    public void Cancel()
    {
        // AC: An event with the status Completed cannot be cancelled
        if (Status == EventStatus.Completed)
            throw new InvalidOperationException("Cannot cancel an event that is already completed.");

        // AC: An event with the status Published can be cancelled 
        
        if (Status != EventStatus.Published && Status != EventStatus.Draft)
            throw new InvalidOperationException("Only Published or Draft events can be cancelled.");

        Status = EventStatus.Cancelled;

        // AC: When an event is cancelled, all ticket categories can no longer be purchased
        foreach (var category in _categories)
        {
            category.Deactivate(); // Menggunakan method Deactivate yang kita buat sebelumnya
        }

        // AC: Raise domain event EventCancelled
        _domainEvents.Add(new EventCancelled(Id));
    }

    public void DisableTicketCategory(Guid categoryId)
    {
        // AC: A ticket category can be disabled if the event has not been completed
        if (Status == EventStatus.Completed)
            throw new InvalidOperationException("Cannot disable categories for a completed event.");

        var category = _categories.FirstOrDefault(c => c.Id == categoryId);
        if (category == null)
            throw new Exception("Ticket category not found.");

        // Eksekusi deaktifasi
        category.Deactivate();

        // AC: Raise domain event TicketCategoryDisabled
        _domainEvents.Add(new TicketCategoryDisabled(Id, categoryId));
    }
}
