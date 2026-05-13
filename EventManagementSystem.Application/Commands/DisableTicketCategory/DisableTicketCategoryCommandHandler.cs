using EventManagementSystem.Domain.Repositories;
using MediatR;

namespace EventManagementSystem.Application.Commands.DisableTicketCategory;

public class DisableTicketCategoryCommandHandler : IRequestHandler<DisableTicketCategoryCommand>
{
    private readonly IEventRepository _eventRepository;

    public DisableTicketCategoryCommandHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task Handle(DisableTicketCategoryCommand request, CancellationToken ct)
    {
        var @event = await _eventRepository.GetByIdAsync(request.EventId);
        if (@event == null) throw new Exception("Event not found.");

        // Panggil logic di Domain
        @event.DisableTicketCategory(request.CategoryId);

        await _eventRepository.SaveAsync(@event);
    }
}