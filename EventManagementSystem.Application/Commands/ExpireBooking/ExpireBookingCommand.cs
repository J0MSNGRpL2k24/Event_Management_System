using MediatR;

namespace EventManagementSystem.Application.Commands.ExpireBooking;

public record ExpireBookingCommand(Guid BookingId) : IRequest;