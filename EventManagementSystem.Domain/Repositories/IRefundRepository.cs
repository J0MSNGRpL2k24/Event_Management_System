using EventManagementSystem.Domain.Entities;

namespace EventManagementSystem.Domain.Repositories;

public interface IRefundRepository
{
    Task<Refund?> GetByIdAsync(Guid id);
    Task SaveAsync(Refund refund);
}