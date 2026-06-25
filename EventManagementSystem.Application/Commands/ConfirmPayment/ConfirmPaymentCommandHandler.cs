using EventManagementSystem.Domain.Repositories;
using EventManagementSystem.Domain.ValueObjects;
using MediatR;

namespace EventManagementSystem.Application.Commands.ConfirmPayment;

public class ConfirmPaymentCommandHandler : IRequestHandler<ConfirmPaymentCommand>
{
    private readonly IBookingRepository _bookingRepository;

    public ConfirmPaymentCommandHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
        if (booking == null) throw new Exception("Booking not found.");

        var payment = new Money(request.AmountPaid, "IDR");

        booking.ConfirmPayment(payment);

        await _bookingRepository.SaveAsync(booking);
    }
}