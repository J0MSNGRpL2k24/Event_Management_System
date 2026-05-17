using MediatR;

namespace EventManagementSystem.Application.Commands.RejectRefund;

public record RejectRefundCommand(Guid RefundId, string RejectionReason) : IRequest;