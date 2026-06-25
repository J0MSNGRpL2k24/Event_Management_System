using EventManagementSystem.Domain.Repositories;
using MediatR;

namespace EventManagementSystem.Application.Commands.ExpireBooking;

public class ExpireBookingCommandHandler : IRequestHandler<ExpireBookingCommand>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IEventRepository _eventRepository;

    public ExpireBookingCommandHandler(IBookingRepository bookingRepository, IEventRepository eventRepository)
    {
        _bookingRepository = bookingRepository;
        _eventRepository = eventRepository;
    }

    public async Task Handle(ExpireBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
        if (booking == null) throw new Exception("Booking not found.");

        booking.Expire();

        var @event = await _eventRepository.GetByIdAsync(booking.EventId);
        if (@event != null)
        {
            @event.ReleaseTicketQuota(booking.CategoryId, booking.Quantity);
            await _eventRepository.SaveAsync(@event);
        }

        await _bookingRepository.SaveAsync(booking);
    }
}