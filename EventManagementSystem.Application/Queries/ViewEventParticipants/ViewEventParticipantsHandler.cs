using EventManagementSystem.Domain.Entities;
using EventManagementSystem.Domain.Repositories;
using MediatR;

namespace EventManagementSystem.Application.Queries.ViewEventParticipants;

public class ViewEventParticipantsHandler : IRequestHandler<ViewEventParticipantsQuery, List<EventParticipantDto>>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IEventRepository _eventRepository;

    public ViewEventParticipantsHandler(IBookingRepository bookingRepository, IEventRepository eventRepository)
    {
        _bookingRepository = bookingRepository;
        _eventRepository = eventRepository;
    }

    public async Task<List<EventParticipantDto>> Handle(ViewEventParticipantsQuery request, CancellationToken cancellationToken)
    {
        var @event = await _eventRepository.GetByIdAsync(request.EventId);
        if (@event == null) throw new Exception("Event not found.");

        var bookings = await _bookingRepository.GetByEventIdAsync(request.EventId);

        var paidBookings = bookings.Where(b => b.Status == BookingStatus.Paid).ToList();

        var participants = new List<EventParticipantDto>();

        foreach (var booking in paidBookings)
        {
            string customerName = $"Customer-{booking.CustomerId.ToString().Substring(0, 5).ToUpper()}";

            var categoryName = @event.Categories.FirstOrDefault(c => c.Id == booking.CategoryId)?.Name ?? "Unknown";

            foreach (var ticket in booking.Tickets)
            {
                participants.Add(new EventParticipantDto(
                    customerName,
                    categoryName,
                    ticket.TicketCode,
                    ticket.Status.ToString()
                ));
            }
        }

        return participants;
    }
}