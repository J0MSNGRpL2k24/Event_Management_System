using MediatR;

namespace EventManagementSystem.Application.Queries.ViewEventSalesReport;

public record ViewEventSalesReportQuery(Guid EventId) : IRequest<EventSalesReportDto>;