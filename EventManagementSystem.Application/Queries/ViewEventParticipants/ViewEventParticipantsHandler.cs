using EventManagementSystem.Domain.Entities;
using EventManagementSystem.Domain.Repositories;
using MediatR;

namespace EventManagementSystem.Application.Queries.ViewEventParticipants;

public class ViewEventParticipantsHandler : IRequestHandler<ViewEventParticipantsQuery, List<EventParticipantDto>>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IEventRepository _eventRepository; // Tambahkan ini

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

        // AC 1 & AC 2: Only displays customers from bookings with the status Paid. 
        // Participants from refunded bookings are NOT displayed.
        var paidBookings = bookings.Where(b => b.Status == BookingStatus.Paid).ToList();

        var participants = new List<EventParticipantDto>();

        foreach (var booking in paidBookings)
        {
            // AC 3: Customer name (Simulasi karena kita tidak join ke tabel Customer)
            string customerName = $"Customer-{booking.CustomerId.ToString().Substring(0, 5).ToUpper()}";

            // Ambil nama kategori yang dibeli
            var categoryName = @event.Categories.FirstOrDefault(c => c.Id == booking.CategoryId)?.Name ?? "Unknown";

            // Loop setiap tiket fisik yang dimiliki oleh booking ini
            foreach (var ticket in booking.Tickets)
            {
                participants.Add(new EventParticipantDto(
                    customerName,
                    categoryName, // AC 3: Ticket category
                    ticket.TicketCode, // AC 3: Ticket code
                    ticket.Status.ToString() // AC 3: Check-in status
                ));
            }
        }

        return participants;
    }
}