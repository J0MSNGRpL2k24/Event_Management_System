using EventManagementSystem.Domain.Repositories;
using MediatR;

namespace EventManagementSystem.Application.Commands.CancelEvent;

public class CancelEventCommandHandler : IRequestHandler<CancelEventCommand>
{
    private readonly IEventRepository _eventRepository;
    private readonly IBookingRepository _bookingRepository;

    public CancelEventCommandHandler(IEventRepository eventRepository, IBookingRepository bookingRepository)
    {
        _eventRepository = eventRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task Handle(CancelEventCommand request, CancellationToken cancellationToken)
    {
        var @event = await _eventRepository.GetByIdAsync(request.EventId);
        if (@event == null) throw new Exception("Event not found.");

      
        @event.Cancel();

        
        await _eventRepository.SaveAsync(@event);

        
        await _bookingRepository.MarkBookingsForRefundAsync(request.EventId);
    }
}