using MediatR;

namespace EventManagementSystem.Application.Commands.CreateBooking;

public record CreateBookingCommand(
    Guid CustomerId,
    Guid EventId,
    Guid CategoryId,
    int Quantity
) : IRequest<Guid>;