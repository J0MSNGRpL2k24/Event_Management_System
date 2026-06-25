namespace EventManagementSystem.Domain.Entities;

public class Customer
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }

    private Customer()
    {
        Name = null!;
        Email = null!;
    }

    public Customer(string name, string email)
    {
        Id = Guid.NewGuid();
        Name = name;
        Email = email;
    }
}