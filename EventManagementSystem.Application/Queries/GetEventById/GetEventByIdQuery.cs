using EventManagementSystem.Domain.Entities;
using MediatR;

namespace EventManagementSystem.Application.Queries.GetEventById;

public record GetEventByIdQuery(Guid Id) : IRequest<Event?>;