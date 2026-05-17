using MediatR;

namespace EventManagementSystem.Application.Commands.MarkRefundAsPaidOut;

public record MarkRefundAsPaidOutCommand(Guid RefundId, string PaymentReference) : IRequest;