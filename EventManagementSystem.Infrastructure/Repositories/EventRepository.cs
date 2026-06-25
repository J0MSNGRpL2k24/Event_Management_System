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
        
        if (_context.Entry(@event).State == EntityState.Detached)
        {
            await _context.Events.AddAsync(@event);
        }

        
        
       
        await _context.SaveChangesAsync();
    }
}