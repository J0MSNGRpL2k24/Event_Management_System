using EventManagementSystem.Domain.Entities;
using EventManagementSystem.Domain.Repositories;
using EventManagementSystem.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using System;

namespace EventManagementSystem.Infrastructure.Repositories;

public class EventRepository : IEventRepository
{
    private readonly AppDbContext _context;

    public EventRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Event?> GetByIdAsync(Guid id)
    {
        //  menggunakan Include() agar list TicketCategory ikut ditarik dari DB
        return await _context.Events
            .Include(e => e.Categories)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<List<Event>> GetAllAsync()
    {
        return await _context.Events
            .Include(e => e.Categories)
            .ToListAsync();
    }

    public async Task SaveAsync(Event @event)
    {
        // Jika entity belum di-track oleh Entity Framework, maka tambahkan ke context. Jika sudah di-track, maka update.
        if (!_context.Events.Local.Any(e => e.Id == @event.Id))
        {
            _context.Events.Update(@event);
        }
        await _context.SaveChangesAsync();
    }
}