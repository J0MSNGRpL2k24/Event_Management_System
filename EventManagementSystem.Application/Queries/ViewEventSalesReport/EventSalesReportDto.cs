namespace EventManagementSystem.Application.Queries.ViewEventSalesReport;

// DTO for sales per ticket category (AC 1)
public record TicketCategorySalesDto(string CategoryName, int TicketsSold);

// DTO for booking status statistics (AC 2)
public record BookingStatusStatsDto(string Status, int Count);

// Core DTO for the event sales report
public record EventSalesReportDto(
    Guid EventId,
    decimal TotalRevenue, // AC 3: Total revenue from paid bookings
    List<TicketCategorySalesDto> TicketsSoldPerCategory,
    List<BookingStatusStatsDto> BookingStatistics
);