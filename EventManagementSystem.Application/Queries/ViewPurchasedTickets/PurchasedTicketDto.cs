namespace EventManagementSystem.Application.Queries.ViewPurchasedTickets;

public record PurchasedTicketDto(
    Guid TicketId,
    Guid EventId,
    string EventName,
    DateTime EventDate,
    string TicketCode, // AC: Each ticket must have a unique ticket code.
    string Status      // AC: Active, CheckedIn, Cancelled, or RefundRequired
);