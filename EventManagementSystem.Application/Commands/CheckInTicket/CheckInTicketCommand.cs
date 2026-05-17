using MediatR;
public record CheckInTicketCommand(string TicketCode, Guid EventId) : IRequest;