using EventManagementSystem.Domain.Entities;

namespace EventManagementSystem.Domain.Repositories;

public interface ITicketRepository
{
   
    Task<Ticket?> GetByCodeAsync(string ticketCode);

    
    Task SaveAsync(Ticket ticket);
}