using MediatR;
public record RequestRefundCommand(Guid BookingId, string Reason) : IRequest<Guid>;