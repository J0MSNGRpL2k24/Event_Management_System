using EventManagementSystem.Domain.Entities;
using EventManagementSystem.Domain.Repositories;
using MediatR;

namespace EventManagementSystem.Application.Commands.CreateBooking;

public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, Guid>
{
    private readonly IEventRepository _eventRepository;
    private readonly IBookingRepository _bookingRepository;

    public CreateBookingCommandHandler(IEventRepository eventRepository, IBookingRepository bookingRepository)
    {
        _eventRepository = eventRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task<Guid> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        // AC: Customer cannot have more than one active booking for the same event
        if (await _bookingRepository.HasActiveBookingAsync(request.CustomerId, request.EventId))
            throw new InvalidOperationException("You already have an active booking for this event.");

        var @event = await _eventRepository.GetByIdAsync(request.EventId);
        if (@event == null) throw new Exception("Event not found.");

        var category = @event.Categories.FirstOrDefault(c => c.Id == request.CategoryId);
        if (category == null) throw new Exception("Category not found.");

     
        @event.ReserveTickets(request.CategoryId, request.Quantity);

       
        var booking = Booking.Create(request.CustomerId, @event, category, request.Quantity);

      
        await _eventRepository.SaveAsync(@event);
        await _bookingRepository.SaveAsync(booking);

        

        return booking.Id;
    }
}