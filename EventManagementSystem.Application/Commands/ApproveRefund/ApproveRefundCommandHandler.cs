using EventManagementSystem.Domain.Repositories;
using MediatR;

namespace EventManagementSystem.Application.Commands.ApproveRefund;

public class ApproveRefundCommandHandler : IRequestHandler<ApproveRefundCommand>
{
    private readonly IRefundRepository _refundRepository;
    private readonly IBookingRepository _bookingRepository;

    public ApproveRefundCommandHandler(IRefundRepository refundRepository, IBookingRepository bookingRepository)
    {
        //
        _refundRepository = refundRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task Handle(ApproveRefundCommand request, CancellationToken cancellationToken)
    {
        var refund = await _refundRepository.GetByIdAsync(request.RefundId);
        if (refund == null) throw new Exception("Refund request not found.");

        var booking = await _bookingRepository.GetByIdAsync(refund.BookingId);
        if (booking == null) throw new Exception("Related booking not found.");

        refund.Approve();           
        booking.MarkAsRefunded(); 

        
        await _refundRepository.SaveAsync(refund);
        await _bookingRepository.SaveAsync(booking);
    }
}