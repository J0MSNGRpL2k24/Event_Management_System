namespace EventManagementSystem.Application.Queries.ViewEventParticipants;

public record EventParticipantDto(
    string CustomerName,
    string TicketCategory,
    string TicketCode,
    string CheckInStatus
);