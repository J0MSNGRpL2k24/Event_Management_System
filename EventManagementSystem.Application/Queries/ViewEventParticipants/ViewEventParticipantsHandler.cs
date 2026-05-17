using EventManagementSystem.Domain.Entities;
using EventManagementSystem.Domain.Repositories;
using MediatR;

namespace EventManagementSystem.Application.Queries.ViewEventParticipants;

public class ViewEventParticipantsHandler : IRequestHandler<ViewEventParticipantsQuery, List<EventParticipantDto>>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ICustomerRepository _customerRepository;

    public ViewEventParticipantsHandler(IBookingRepository bookingRepository, ICustomerRepository customerRepository)
    {
        _bookingRepository = bookingRepository;
        _customerRepository = customerRepository;
    }

    public async Task<List<EventParticipantDto>> Handle(ViewEventParticipantsQuery request, CancellationToken cancellationToken)
    {
        var bookings = await _bookingRepository.GetByEventIdAsync(request.EventId);

        // AC 1 & 2: Only displays customers from bookings with the status Paid.
        // (Automated) AC 4: Bookings with status other than Paid are not included in the participant list.
        var paidBookings = bookings.Where(b => b.Status == BookingStatus.Paid).ToList();

        var participants = new List<EventParticipantDto>();

        foreach (var booking in paidBookings)
        {
            // Ambil data customer untuk mendapatkan namanya (AC 3)
            var customer = await _customerRepository.GetByIdAsync(booking.CustomerId);
            string customerName = customer?.Name ?? "Unknown Customer";

            foreach (var ticket in booking.Tickets)
            {

                // Asumsi ticket category = "General" karena tidak ada informasi lebih lanjut tentang kategori tiket dalam domain model.
                string ticketCategory = "General";

                // AC 3: Participant data includes name, category, code, and status.
                participants.Add(new EventParticipantDto(
                    customerName,
                    ticketCategory,
                    ticket.TicketCode,
                    ticket.Status.ToString() // Display check-in status as string (e.g., "CheckedIn", "NotCheckedIn")
                ));
            }
        }

        return participants;
    }
}