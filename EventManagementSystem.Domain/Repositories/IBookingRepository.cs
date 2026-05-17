using EventManagementSystem.Domain.Entities;

namespace EventManagementSystem.Domain.Repositories;

public interface IBookingRepository
{
    Task<bool> HasActiveBookingAsync(Guid customerId, Guid eventId);
    Task SaveAsync(Booking booking);
    Task MarkBookingsForRefundAsync(Guid eventId);
    Task<Booking?> GetByIdAsync(Guid id);

    Task<List<Booking>> GetByCustomerIdAsync(Guid customerId);
    Task<List<Booking>> GetByEventIdAsync(Guid eventId);



}