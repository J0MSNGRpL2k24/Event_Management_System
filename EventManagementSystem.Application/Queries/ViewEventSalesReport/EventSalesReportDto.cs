namespace EventManagementSystem.Application.Queries.ViewEventSalesReport;

public record TicketCategorySalesDto(string CategoryName, int TicketsSold);

public record BookingStatusStatsDto(string Status, int Count);

public record EventSalesReportDto(
    Guid EventId,
    decimal TotalRevenue,
    List<TicketCategorySalesDto> TicketsSoldPerCategory,
    List<BookingStatusStatsDto> BookingStatistics
);