using EventManagementSystem.Domain.Repositories;
using MediatR;

namespace EventManagementSystem.Application.Commands.ApproveRefund;

public class ApproveRefundCommandHandler : IRequestHandler<ApproveRefundCommand>
{
    private readonly IRefundRepository _refundRepository;
    private readonly IBookingRepository _bookingRepository;

    public ApproveRefundCommandHandler(IRefundRepository refundRepository, IBookingRepository bookingRepository)
    {
        _refundRepository = refundRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task Handle(ApproveRefundCommand request, CancellationToken cancellationToken)
    {
        // 1. get  Refund data and validate if refund request exists
        var refund = await _refundRepository.GetByIdAsync(request.RefundId);
        if (refund == null) throw new Exception("Refund request not found.");

        // 2.get related booking data and validate if booking exists
        var booking = await _bookingRepository.GetByIdAsync(refund.BookingId);
        if (booking == null) throw new Exception("Related booking not found.");

        // 3. Domain logic for approve refund
        refund.Approve();           
        booking.MarkAsRefunded(); 

        // 4. Save to DB
        await _refundRepository.SaveAsync(refund);
        await _bookingRepository.SaveAsync(booking);
    }
}