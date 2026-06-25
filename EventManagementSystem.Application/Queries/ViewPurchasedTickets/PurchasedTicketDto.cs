namespace EventManagementSystem.Application.Queries.ViewPurchasedTickets;

public record PurchasedTicketDto(
    Guid TicketId,
    Guid EventId,
    string EventName,
    DateTime EventDate,
    string TicketCode,
    string Status
);