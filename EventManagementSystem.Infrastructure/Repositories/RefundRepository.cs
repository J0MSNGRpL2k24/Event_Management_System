using EventManagementSystem.Domain.Entities;
using EventManagementSystem.Domain.Repositories;
using EventManagementSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventManagementSystem.Infrastructure.Repositories;

public class RefundRepository : IRefundRepository
{
    private readonly AppDbContext _context;

    public RefundRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Refund?> GetByIdAsync(Guid id)
    {
        return await _context.Refunds.FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Refund?> GetByBookingIdAsync(Guid bookingId)
    {
        return await _context.Refunds.FirstOrDefaultAsync(r => r.BookingId == bookingId);
    }

    public async Task SaveAsync(Refund refund)
    {
        if (!_context.Refunds.Local.Any(r => r.Id == refund.Id))
        {
            _context.Refunds.Update(refund);
        }
        await _context.SaveChangesAsync();
    }
}