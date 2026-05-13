using MediatR;

namespace EventManagementSystem.Application.Commands.CreateTicketCategory;

public record CreateTicketCategoryCommand(
    Guid EventId,
    string Name,
    decimal PriceAmount,
    int Quota,
    DateTime SalesStart,
    DateTime SalesEnd
) : IRequest<Guid>;