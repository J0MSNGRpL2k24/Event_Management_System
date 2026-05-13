using EventManagementSystem.Domain.ValueObjects;

namespace EventManagementSystem.Domain.Entities;

public class TicketCategory
{
    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public string Name { get; private set; }
    public Money Price { get; private set; }
    public int Quota { get; private set; }
    public int RemainingQuota { get; private set; }
    public DateTime SalesStart { get; private set; }
    public DateTime SalesEnd { get; private set; }
    public bool IsActive { get; private set; }


    internal TicketCategory(Guid eventId, string name, Money price, int quota, DateTime salesStart, DateTime salesEnd)
    {
        Id = Guid.NewGuid();
        EventId = eventId;
        Name = name;
        Price = price;
        Quota = quota;
        RemainingQuota = quota;
        SalesStart = salesStart;
        SalesEnd = salesEnd;
        IsActive = true;
    }
    public void ReserveTickets(int quantity)
    {
        if (quantity > RemainingQuota)
            throw new InvalidOperationException("Not enough tickets remaining.");

        RemainingQuota -= quantity;
    }


    public void Deactivate()
    {
        IsActive = false;
    }
}