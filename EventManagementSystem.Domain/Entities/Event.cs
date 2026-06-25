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
        if (maxCapacity <= 0)
            throw new ArgumentException("Maximum capacity must be greater than zero.");

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
            Status = EventStatus.Draft
        };

        @event._domainEvents.Add(new EventCreated(@event.Id, @event.Name));

        return @event; }

        public void AddTicketCategory(string name, Money price, int quota, DateTime salesStart, DateTime salesEnd)
    {
        if (quota <= 0)
            throw new ArgumentException("Ticket quota must be greater than zero.");

        int currentTotalQuota = _categories.Sum(c => c.Quota);
        if (currentTotalQuota + quota > MaxCapacity)
            throw new InvalidOperationException("Total quota of all categories exceeds event capacity.");

        if (salesEnd > StartDate)
            throw new ArgumentException("Ticket sales period must end before the event starts.");

        var category = new TicketCategory(Id, name, price, quota, salesStart, salesEnd);
        _categories.Add(category);

        _domainEvents.Add(new TicketCategoryCreated(Id, category.Id, name));
    }


    public void Publish()
    {
        if (Status == EventStatus.Cancelled)
            throw new InvalidOperationException("A cancelled event cannot be published.");

        if (Status != EventStatus.Draft)
            throw new InvalidOperationException("Only draft events can be published.");

        if (!_categories.Any(c => c.IsActive))
            throw new InvalidOperationException("Event must have at least one active ticket category to be published.");

        int totalQuota = _categories.Sum(c => c.Quota);
        if (totalQuota > MaxCapacity)
            throw new InvalidOperationException("Total ticket quota exceeds event capacity.");

        Status = EventStatus.Published;

        _domainEvents.Add(new EventPublished(Id));
    }

    public void ReserveTickets(Guid categoryId, int quantity)
    {
        if (Status != EventStatus.Published)
            throw new InvalidOperationException("Cannot reserve tickets for an unpublished event.");

        var category = _categories.FirstOrDefault(c => c.Id == categoryId);

        if (category == null || !category.IsActive)
            throw new InvalidOperationException("Ticket category is not available.");

        var now = DateTime.UtcNow;
        if (now < category.SalesStart || now > category.SalesEnd)
            throw new InvalidOperationException("Ticket is outside of sales period.");

        category.ReserveTickets(quantity);
    }

    public void Cancel()
    {
        if (Status == EventStatus.Completed)
        {
            throw new InvalidOperationException("Cannot cancel an event that is already completed.");
        }

        if (Status != EventStatus.Published && Status != EventStatus.Draft)
        {
            throw new InvalidOperationException("Only Published or Draft events can be cancelled.");
        }

        Status = EventStatus.Cancelled;

        foreach (var category in _categories)
        {
            category.Deactivate();
        }

        _domainEvents.Add(new EventCancelled(Id));
    }

    public void DisableTicketCategory(Guid categoryId)
    {
        if (Status == EventStatus.Completed)
            throw new InvalidOperationException("Cannot disable categories for a completed event.");

        var category = _categories.FirstOrDefault(c => c.Id == categoryId);
        if (category == null)
            throw new Exception("Ticket category not found.");

        category.Deactivate();

        _domainEvents.Add(new TicketCategoryDisabled(Id, categoryId));
    }


    public void ReleaseTicketQuota(Guid categoryId, int quantity)
    {
        var category = _categories.FirstOrDefault(c => c.Id == categoryId);
        if (category == null)
            throw new Exception("Ticket category not found.");

        category.ReleaseQuota(quantity);
    }
    public void Complete()
    {
        Status = EventStatus.Completed;
    }

    public void ResetToPublishedForTest()
    {
        Status = EventStatus.Published;
    }
}
