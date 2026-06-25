using EventManagementSystem.Domain.Repositories;
using MediatR;

namespace EventManagementSystem.Application.Commands.CheckInTicket;

public class CheckInTicketCommandHandler : IRequestHandler<CheckInTicketCommand>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IEventRepository _eventRepository;

    public CheckInTicketCommandHandler(ITicketRepository ticketRepository, IEventRepository eventRepository)
    {
        _ticketRepository = ticketRepository;
        _eventRepository = eventRepository;
    }

    public async Task Handle(CheckInTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByCodeAsync(request.TicketCode);

        if (ticket == null)
            throw new Exception("The ticket is invalid.");
        var @event = await _eventRepository.GetByIdAsync(request.EventId);

        if (@event == null)
            throw new Exception("Event not found."); 

        
        ticket.CheckIn(@event);

        await _ticketRepository.SaveAsync(ticket);
    }
}