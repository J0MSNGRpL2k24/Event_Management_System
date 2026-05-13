using EventManagementSystem.Domain.Entities;
using EventManagementSystem.Domain.Repositories;
using MediatR; 

namespace EventManagementSystem.Application.Commands.CreateEvent;

public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, Guid>
{
    private readonly IEventRepository _eventRepository;

    public CreateEventCommandHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<Guid> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var @event = Event.Create(
            request.OrganizerId,
            request.Name,
            request.Description,
            request.StartDate,
            request.EndDate,
            request.Location,
            request.MaxCapacity
        );

        await _eventRepository.SaveAsync(@event);

        return @event.Id;
    }
}