using EventManagementSystem.Domain.Entities;
using EventManagementSystem.Domain.Repositories;
using MediatR;

namespace EventManagementSystem.Application.Queries.ViewEventParticipants;

public class ViewEventParticipantsHandler : IRequestHandler<ViewEventParticipantsQuery, List<EventParticipantDto>>
{
    // Hapus ICustomerRepository, cukup sisakan IBookingRepository
    private readonly IBookingRepository _bookingRepository;

    public ViewEventParticipantsHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<List<EventParticipantDto>> Handle(ViewEventParticipantsQuery request, CancellationToken cancellationToken)
    {
        var bookings = await _bookingRepository.GetByEventIdAsync(request.EventId);
        var paidBookings = bookings.Where(b => b.Status == BookingStatus.Paid).ToList();
        var participants = new List<EventParticipantDto>();

        foreach (var booking in paidBookings)
        {
            
            string customerName = $"Guest-{booking.CustomerId.ToString().Substring(0, 4).ToUpper()}";

            foreach (var ticket in booking.Tickets)
            {
                participants.Add(new EventParticipantDto(
                    customerName,
                    "General",
                    ticket.TicketCode,
                    ticket.Status.ToString()
                ));
            }
        }
        return participants;
    }
}