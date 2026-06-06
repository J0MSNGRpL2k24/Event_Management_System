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
        
        var exists = await _context.Refunds.AnyAsync(r => r.Id == refund.Id);

        if (!exists)
        {
           
            await _context.Refunds.AddAsync(refund);
        }
        else
        {
            _context.Refunds.Update(refund);
        }

        await _context.SaveChangesAsync();
    }
}