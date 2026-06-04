using EventManagementSystem.Domain.Entities;
using EventManagementSystem.Domain.Repositories;
using EventManagementSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventManagementSystem.Infrastructure.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly AppDbContext _context;

    public TicketRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Ticket?> GetByCodeAsync(string ticketCode)
    {
        return await _context.Tickets.FirstOrDefaultAsync(t => t.TicketCode == ticketCode);
    }

    public async Task SaveAsync(Ticket ticket)
    {
        if (!_context.Tickets.Local.Any(t => t.Id == ticket.Id))
        {
            _context.Tickets.Update(ticket);
        }
        await _context.SaveChangesAsync();
    }
}