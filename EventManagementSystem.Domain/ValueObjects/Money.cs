namespace EventManagementSystem.Domain.ValueObjects;

public record Money
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }

    public Money(decimal amount, string currency = "IDR")
    {
        if (amount < 0)
            throw new ArgumentException("Ticket price cannot be less than zero.");

        Amount = amount;
        Currency = currency;
    }
}