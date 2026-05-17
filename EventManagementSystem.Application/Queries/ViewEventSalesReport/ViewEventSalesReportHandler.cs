using EventManagementSystem.Domain.Entities;
using EventManagementSystem.Domain.Repositories;
using MediatR;

namespace EventManagementSystem.Application.Queries.ViewEventSalesReport;

public class ViewEventSalesReportHandler : IRequestHandler<ViewEventSalesReportQuery, EventSalesReportDto>
{
    private readonly IBookingRepository _bookingRepository;

    public ViewEventSalesReportHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<EventSalesReportDto> Handle(ViewEventSalesReportQuery request, CancellationToken cancellationToken)
    {
        // Fetch all bookings for the specified event
        var bookings = await _bookingRepository.GetByEventIdAsync(request.EventId);

        // AC 2: Calculate the number of bookings per status
        var bookingStats = bookings
            .GroupBy(b => b.Status)
            .Select(g => new BookingStatusStatsDto(g.Key.ToString(), g.Count()))
            .ToList();

        // Fetch only for paid bookings to calculate revenue and ticket sales
        var paidBookings = bookings.Where(b => b.Status == BookingStatus.Paid).ToList();

        // AC 3: Hitung total revenue dari paid bookings

        decimal totalRevenue = paidBookings.Sum(b => b.TotalPrice.Amount);
        // AC 1: Calculate the number of tickets sold per category (assuming all tickets are "General" for simplicity)

        var ticketStats = paidBookings
            .SelectMany(b => b.Tickets)
            .GroupBy(t => "General") 
            .Select(g => new TicketCategorySalesDto(g.Key, g.Count()))
            .ToList();

        return new EventSalesReportDto(
            request.EventId,
            totalRevenue,
            ticketStats,
            bookingStats
        );
    }
}