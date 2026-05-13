using MediatR;

namespace EventManagementSystem.Application.Queries.GetAvailableEvents;

public record GetAvailableEventsQuery(DateTime? FilterDate = null, string? FilterLocation = null)
    : IRequest<List<EventSummaryDto>>;