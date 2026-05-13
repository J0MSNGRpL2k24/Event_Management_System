using MediatR;

namespace EventManagementSystem.Application.Commands.DisableTicketCategory;

public record DisableTicketCategoryCommand(Guid EventId, Guid CategoryId) : IRequest;