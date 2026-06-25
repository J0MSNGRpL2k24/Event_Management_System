using EventManagementSystem.Domain.Repositories;
using MediatR;

namespace EventManagementSystem.Application.Commands.MarkRefundAsPaidOut;

public class MarkRefundAsPaidOutCommandHandler : IRequestHandler<MarkRefundAsPaidOutCommand>
{
    private readonly IRefundRepository _refundRepository;

    public MarkRefundAsPaidOutCommandHandler(IRefundRepository refundRepository)
    {
        _refundRepository = refundRepository;
    }

    public async Task Handle(MarkRefundAsPaidOutCommand request, CancellationToken cancellationToken)
    {
        var refund = await _refundRepository.GetByIdAsync(request.RefundId);
        if (refund == null) throw new Exception("Refund request not found.");

        refund.MarkAsPaidOut(request.PaymentReference);

        await _refundRepository.SaveAsync(refund);
    }
}