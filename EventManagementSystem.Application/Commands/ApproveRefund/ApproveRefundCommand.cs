using MediatR;
public record ApproveRefundCommand(Guid RefundId) : IRequest;