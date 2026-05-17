using EventManagementSystem.Domain.Repositories;
using MediatR;

namespace EventManagementSystem.Application.Commands.RejectRefund;

public class RejectRefundCommandHandler : IRequestHandler<RejectRefundCommand>
{
    private readonly IRefundRepository _refundRepository;

    public RejectRefundCommandHandler(IRefundRepository refundRepository)
    {
        _refundRepository = refundRepository;
    }

    public async Task Handle(RejectRefundCommand request, CancellationToken cancellationToken)
    {
        var refund = await _refundRepository.GetByIdAsync(request.RefundId);
        if (refund == null) throw new Exception("Refund request not found.");

        // Panggil domain logic
        refund.Reject(request.RejectionReason);

        // Simpan perubahan status dan alasan penolakan
        await _refundRepository.SaveAsync(refund);
    }
}