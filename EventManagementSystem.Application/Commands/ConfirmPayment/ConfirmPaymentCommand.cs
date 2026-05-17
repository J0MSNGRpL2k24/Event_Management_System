using MediatR;

namespace EventManagementSystem.Application.Commands.ConfirmPayment;


public record ConfirmPaymentCommand(Guid BookingId, decimal AmountPaid) : IRequest;