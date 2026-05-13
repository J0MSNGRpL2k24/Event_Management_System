using EventManagementSystem.Domain.Repositories;
using MediatR;

namespace EventManagementSystem.Application.Commands.PublishEvent;

public class PublishEventCommandHandler : IRequestHandler<PublishEventCommand>
{
    private readonly IEventRepository _eventRepository;

    public PublishEventCommandHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task Handle(PublishEventCommand request, CancellationToken cancellationToken)
    {
        var @event = await _eventRepository.GetByIdAsync(request.EventId);

        if (@event == null)
            throw new Exception("Event not found.");

       
        @event.Publish();

        
        await _eventRepository.SaveAsync(@event);
    }
}