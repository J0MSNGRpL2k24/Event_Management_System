using EventManagementSystem.Domain.Entities;
using EventManagementSystem.Domain.Events;
using EventManagementSystem.Domain.ValueObjects;
using Xunit;
using System.Linq;

namespace EventManagementSystem.Domain.Tests;

public class TicketCategoryTests
{
    private Event CreateValidEvent()
    {
        var organizerId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(10);
        var endDate = DateTime.UtcNow.AddDays(12);
        
        return Event.Create(organizerId, "Tech Fest", "Desc", startDate, endDate, "Graha ITS", 1000);
    }


    [Fact]
    public void AddTicketCategory_ValidData_ShouldCreateCategoryAndRaiseEvent()
    {
        var @event = CreateValidEvent();
        var salesStart = DateTime.UtcNow;
        var salesEnd = @event.StartDate.AddDays(-1);

        @event.AddTicketCategory("VIP", new Money(150000), 500, salesStart, salesEnd);

        Assert.Single(@event.Categories);
        Assert.Contains(@event.DomainEvents, e => e is TicketCategoryCreated);
    }

    [Fact]
    public void AddTicketCategory_QuotaZeroOrLess_ShouldThrowException()
    {
        var @event = CreateValidEvent();

        var ex = Assert.Throws<ArgumentException>(() => 
            @event.AddTicketCategory("VIP", new Money(150000), 0, DateTime.UtcNow, @event.StartDate.AddDays(-1)));
        
        Assert.Contains("greater than zero", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AddTicketCategory_SalesEndAfterEventStart_ShouldThrowException()
    {
        var @event = CreateValidEvent();
        var salesStart = DateTime.UtcNow;
        var salesEnd = @event.StartDate.AddDays(1);

        var ex = Assert.Throws<ArgumentException>(() => 
            @event.AddTicketCategory("VIP", new Money(150000), 500, salesStart, salesEnd));
        
        Assert.Contains("before the event starts", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AddTicketCategory_TotalQuotaExceedsMaxCapacity_ShouldThrowException()
    {
        var @event = CreateValidEvent();
        
        var ex = Assert.Throws<InvalidOperationException>(() => 
            @event.AddTicketCategory("VIP", new Money(150000), 1500, DateTime.UtcNow, @event.StartDate.AddDays(-1)));
        
        Assert.Contains("exceeds event capacity", ex.Message, StringComparison.OrdinalIgnoreCase);
    }


    [Fact]
    public void DisableTicketCategory_ValidCategory_ShouldDeactivateAndRaiseEvent()
    {
        var @event = CreateValidEvent();
        @event.AddTicketCategory("VIP", new Money(150000), 500, DateTime.UtcNow, @event.StartDate.AddDays(-1));
        var categoryId = @event.Categories.First().Id;

        @event.DisableTicketCategory(categoryId);

        var category = @event.Categories.First(c => c.Id == categoryId);
        Assert.False(category.IsActive);
        Assert.Contains(@event.DomainEvents, e => e is TicketCategoryDisabled);
    }

    [Fact]
    public void DisableTicketCategory_CategoryNotFound_ShouldThrowException()
    {
        var @event = CreateValidEvent();
        var randomCategoryId = Guid.NewGuid();

        var ex = Assert.Throws<Exception>(() => 
            @event.DisableTicketCategory(randomCategoryId));
        
        Assert.Contains("not found", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}