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
        // 1. Ambil data booking
        var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
        if (booking == null) throw new Exception("Booking not found.");

        // 2. Eksekusi Expire di Domain Booking
        booking.Expire();

        // 3. Ambil data event terkait untuk melepaskan kuota
        var @event = await _eventRepository.GetByIdAsync(booking.EventId);
        if (@event != null)
        {
            // AC: When a booking expires, the previously reserved ticket quota is released.
            @event.ReleaseTicketQuota(booking.CategoryId, booking.Quantity);
            await _eventRepository.SaveAsync(@event);
        }

        // 4. Simpan perubahan booking
        await _bookingRepository.SaveAsync(booking);
    }
}