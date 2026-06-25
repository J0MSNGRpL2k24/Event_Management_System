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
        var bookings = await _bookingRepository.GetByCustomerIdAsync(request.CustomerId);

        var paidBookings = bookings.Where(b => b.Status == BookingStatus.Paid).ToList();

        var result = new List<PurchasedTicketDto>();

        foreach (var booking in paidBookings)
        {
            var @event = await _eventRepository.GetByIdAsync(booking.EventId);
            if (@event == null) continue;

            foreach (var ticket in booking.Tickets)
            {
                string displayStatus = ticket.Status.ToString();

                if (@event.Status == EventStatus.Cancelled)
                {
                    displayStatus = "RefundRequired";
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