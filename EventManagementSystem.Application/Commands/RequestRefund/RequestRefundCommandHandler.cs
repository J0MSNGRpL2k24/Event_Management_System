using EventManagementSystem.Domain.Entities;
using EventManagementSystem.Domain.Repositories;
using MediatR;

namespace EventManagementSystem.Application.Commands.RequestRefund;

public class RequestRefundCommandHandler : IRequestHandler<RequestRefundCommand, Guid>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IRefundRepository _refundRepository;

    public RequestRefundCommandHandler(
        IBookingRepository bookingRepository,
        IEventRepository eventRepository,
        IRefundRepository refundRepository)
    {
        _bookingRepository = bookingRepository;
        _eventRepository = eventRepository;
        _refundRepository = refundRepository;
    }

    public async Task<Guid> Handle(RequestRefundCommand request, CancellationToken cancellationToken)
    {
        // 1. get booking data and validate if booking exists
        var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
        if (booking == null) throw new Exception("Booking not found.");

        // 2. Ambil data Event terkait
        var @event = await _eventRepository.GetByIdAsync(booking.EventId);
        if (@event == null) throw new Exception("Event not found.");

        // 3. entitas refund and generate domain event di dalam constructor Refund
        var refund = new Refund(booking, @event, request.Reason);

        // 4. Simpan ke database
        await _refundRepository.SaveAsync(refund);

        return refund.Id;
    }
}