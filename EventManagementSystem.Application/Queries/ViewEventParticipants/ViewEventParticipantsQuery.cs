using MediatR;

namespace EventManagementSystem.Application.Queries.ViewEventParticipants;

public record ViewEventParticipantsQuery(Guid EventId) : IRequest<List<EventParticipantDto>>;