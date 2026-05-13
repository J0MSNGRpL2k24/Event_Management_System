using MediatR;

namespace EventManagementSystem.Application.Commands.CancelEvent;

public record CancelEventCommand(Guid EventId) : IRequest;