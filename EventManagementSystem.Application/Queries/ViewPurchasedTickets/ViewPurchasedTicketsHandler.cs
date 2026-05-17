using EventManagementSystem.Domain.Entities;
using EventManagementSystem.Domain.Repositories;
using MediatR;

namespace EventManagementSystem.Application.Queries.ViewPurchasedTickets;

public class ViewPurchasedTicketsHandler : IRequestHandler<ViewPurchasedTicketsQuery, List<PurchasedTicketDto>>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IEventRepository _eventRepository;

    public ViewPurchasedTicketsHandler(IBookingRepository bookingRepository, IEventRepository eventRepository)
    {
        _bookingRepository = bookingRepository;
        _eventRepository = eventRepository;
    }

    public async Task<List<PurchasedTicketDto>> Handle(ViewPurchasedTicketsQuery request, CancellationToken cancellationToken)
    {
        // 1. Ambil semua booking milik customer 
        var bookings = await _bookingRepository.GetByCustomerIdAsync(request.CustomerId);

        // AC: Customers can only view tickets from bookings with the status Paid.
        var paidBookings = bookings.Where(b => b.Status == BookingStatus.Paid).ToList();

        var result = new List<PurchasedTicketDto>();

        foreach (var booking in paidBookings)
        {
            // Ambil data event untuk menampilkan nama, tanggal, dan mengecek status event
            var @event = await _eventRepository.GetByIdAsync(booking.EventId);
            if (@event == null) continue;

            foreach (var ticket in booking.Tickets)
            {
                // AC: Each ticket must have one of the following statuses: Active, CheckedIn, Cancelled.
                string displayStatus = ticket.Status.ToString();

                // AC: Tickets from cancelled events must have the status Cancelled or RefundRequired.
                if (@event.Status == EventStatus.Cancelled)
                {
                    displayStatus = "RefundRequired"; // Proyeksi dinamis, aslinya di DB mungkin masih Active
                }

                result.Add(new PurchasedTicketDto(
                    ticket.Id,
                    @event.Id,
                    @event.Name ?? "Unknown Event",
                    @event.StartDate,
                    ticket.TicketCode,
                    displayStatus
                ));
            }
        }

        return result;
    }
}