using EventManagementSystem.Domain.Entities;

namespace EventManagementSystem.Domain.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id);
}