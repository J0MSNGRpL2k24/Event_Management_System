using EventManagementSystem.Domain.Entities;
using EventManagementSystem.Domain.Repositories;
using MediatR;

namespace EventManagementSystem.Application.Queries.ViewEventSalesReport;

public class ViewEventSalesReportHandler : IRequestHandler<ViewEventSalesReportQuery, EventSalesReportDto>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IEventRepository _eventRepository; // Tambahkan ini

    public ViewEventSalesReportHandler(IBookingRepository bookingRepository, IEventRepository eventRepository)
    {
        _bookingRepository = bookingRepository;
        _eventRepository = eventRepository;
    }

    public async Task<EventSalesReportDto> Handle(ViewEventSalesReportQuery request, CancellationToken cancellationToken)
    {
        // 1. Ambil data Event (untuk mendapatkan nama kategori)
        var @event = await _eventRepository.GetByIdAsync(request.EventId);
        if (@event == null) throw new Exception("Event not found.");

        // 2. Fetch all bookings for the specified event
        var bookings = await _bookingRepository.GetByEventIdAsync(request.EventId);

        // AC 2: Calculate the number of bookings per status (PendingPayment, Paid, Expired, Refunded)
        var bookingStats = bookings
            .GroupBy(b => b.Status)
            .Select(g => new BookingStatusStatsDto(g.Key.ToString(), g.Count()))
            .ToList();

        // AC 3: Hitung total revenue dari PAID bookings
        var paidBookings = bookings.Where(b => b.Status == BookingStatus.Paid).ToList();
        decimal totalRevenue = paidBookings.Sum(b => b.TotalPrice.Amount);

        // AC 1: Calculate the number of tickets sold per category (dengan nama kategori asli)
        var ticketsSoldPerCategory = new List<TicketCategorySalesDto>();

        foreach (var category in @event.Categories)
        {
            // Hitung total tiket (Quantity) dari booking yang Paid berdasarkan CategoryId
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