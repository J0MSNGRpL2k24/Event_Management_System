using EventManagementSystem.Domain.Entities;

namespace EventManagementSystem.Domain.Repositories;

public interface IBookingRepository
{
    Task<bool> HasActiveBookingAsync(Guid customerId, Guid eventId);
    Task SaveAsync(Booking booking);
    Task MarkBookingsForRefundAsync(Guid eventId);
}