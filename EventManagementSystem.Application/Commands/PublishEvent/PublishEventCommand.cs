using MediatR;

namespace EventManagementSystem.Application.Commands.PublishEvent;

public record PublishEventCommand(Guid EventId) : IRequest;