using MediatR;

namespace EventManagementSystem.Application.Queries.GetEventDetail;

public record GetEventDetailQuery(Guid EventId) : IRequest<EventDetailDto?>;