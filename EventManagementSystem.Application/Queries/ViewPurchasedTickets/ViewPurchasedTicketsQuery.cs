using MediatR;

namespace EventManagementSystem.Application.Queries.ViewPurchasedTickets;

public record ViewPurchasedTicketsQuery(Guid CustomerId) : IRequest<List<PurchasedTicketDto>>;