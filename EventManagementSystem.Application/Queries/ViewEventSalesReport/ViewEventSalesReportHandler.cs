using EventManagementSystem.Domain.Entities;
using EventManagementSystem.Domain.Repositories;
using MediatR;

namespace EventManagementSystem.Application.Queries.ViewEventSalesReport;

public class ViewEventSalesReportHandler : IRequestHandler<ViewEventSalesReportQuery, EventSalesReportDto>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IEventRepository _eventRepository;

    public ViewEventSalesReportHandler(IBookingRepository bookingRepository, IEventRepository eventRepository)
    {
        _bookingRepository = bookingRepository;
        _eventRepository = eventRepository;
    }

    public async Task<EventSalesReportDto> Handle(ViewEventSalesReportQuery request, CancellationToken cancellationToken)
    {
        var @event = await _eventRepository.GetByIdAsync(request.EventId);
        if (@event == null) throw new Exception("Event not found.");

        var bookings = await _bookingRepository.GetByEventIdAsync(request.EventId);

        var bookingStats = bookings
            .GroupBy(b => b.Status)
            .Select(g => new BookingStatusStatsDto(g.Key.ToString(), g.Count()))
            .ToList();

        var paidBookings = bookings.Where(b => b.Status == BookingStatus.Paid).ToList();
        decimal totalRevenue = paidBookings.Sum(b => b.TotalPrice.Amount);

        var ticketsSoldPerCategory = new List<TicketCategorySalesDto>();

        foreach (var category in @event.Categories)
        {
            var ticketsSold = paidBookings
                .Where(b => b.CategoryId == category.Id)
                .Sum(b => b.Quantity);

            ticketsSoldPerCategory.Add(new TicketCategorySalesDto(category.Name, ticketsSold));
        }

        return new EventSalesReportDto(
            request.EventId,
            totalRevenue,
            ticketsSoldPerCategory,
            bookingStats
        );
    }
}