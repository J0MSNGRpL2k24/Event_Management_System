using EventManagementSystem.Domain.Entities;
using EventManagementSystem.Domain.Repositories;
using EventManagementSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventManagementSystem.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _context;

    public BookingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Booking?> GetByIdAsync(Guid id)
    {
        return await _context.Bookings
            .Include(b => b.Tickets)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<List<Booking>> GetByCustomerIdAsync(Guid customerId)
    {
        return await _context.Bookings
            .Include(b => b.Tickets)
            .Where(b => b.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task<List<Booking>> GetByEventIdAsync(Guid eventId)
    {
        return await _context.Bookings
            .Include(b => b.Tickets)
            .Where(b => b.EventId == eventId)
            .ToListAsync();
    }

    public async Task<bool> HasActiveBookingAsync(Guid customerId, Guid eventId)
    {
        return await _context.Bookings.AnyAsync(b =>
            b.CustomerId == customerId &&
            b.EventId == eventId &&
            (b.Status == BookingStatus.PendingPayment || b.Status == BookingStatus.Paid));
    }

    public async Task SaveAsync(Booking booking)
    {
        if (_context.Entry(booking).State == EntityState.Detached)
        {
          
            await _context.Bookings.AddAsync(booking);
        }

        await _context.SaveChangesAsync();
    }

    public async Task MarkBookingsForRefundAsync(Guid eventId)
    {
        var bookingsToRefund = await _context.Bookings
            .Include(b => b.Tickets)
            .Where(b => b.EventId == eventId && b.Status == BookingStatus.Paid)
            .ToListAsync();

        foreach (var booking in bookingsToRefund)
        {
            booking.MarkAsRefunded();
        }

        await _context.SaveChangesAsync();
    }
}