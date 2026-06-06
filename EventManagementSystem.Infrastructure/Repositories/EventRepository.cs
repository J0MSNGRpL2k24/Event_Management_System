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
        // Tetap gunakan Include agar tiket lama maupun baru terdeteksi oleh Change Tracker
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
        // Cek apakah Event sudah ada di DB
        var isNew = await _context.Events.AsNoTracking().AnyAsync(e => e.Id == @event.Id) == false;

        if (isNew)
        {
            await _context.Events.AddAsync(@event);
        }
        else
        {
            // PENTING: Jangan gunakan _context.Update(@event) secara membabi buta.
            // Cukup biarkan EF Core melacak perubahan pada koleksi kategori saja.
            foreach (var category in @event.Categories)
            {
                var entry = _context.Entry(category);

                // Jika status tiket adalah Added, tapi di DB sudah ada, 
                // ubah menjadi Unchanged agar tidak di-insert ulang.
                if (entry.State == EntityState.Added)
                {
                    var exists = await _context.Set<TicketCategory>().AnyAsync(c => c.Id == category.Id);
                    if (exists)
                    {
                        entry.State = EntityState.Unchanged;
                    }
                }
            }
        }

        await _context.SaveChangesAsync();
    }
}