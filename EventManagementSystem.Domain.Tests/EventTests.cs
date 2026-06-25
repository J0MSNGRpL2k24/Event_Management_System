using EventManagementSystem.Domain.Entities;
using EventManagementSystem.Domain.Events;
using EventManagementSystem.Domain.ValueObjects;
using Xunit;

namespace EventManagementSystem.Domain.Tests;

public class EventTests
{
    
    [Fact]
    public void CreateEvent_ValidData_ShouldCreateEventWithDraftStatus()
    {
        var startDate = DateTime.UtcNow.AddDays(10);
        var endDate = DateTime.UtcNow.AddDays(12);
        var organizerId = Guid.NewGuid();

        var @event = Event.Create(organizerId, "Tech Fest", "Annual event", startDate, endDate, "Graha ITS", 1000);

        Assert.Equal(EventStatus.Draft, @event.Status);

        Assert.Contains(@event.DomainEvents, e => e is EventCreated);
    }

    [Fact]
    public void CreateEvent_EndDateEarlierThanStartDate_ShouldThrowException()
    {
        var startDate = DateTime.UtcNow.AddDays(10);
        var endDate = DateTime.UtcNow.AddDays(5);
        var organizerId = Guid.NewGuid();

        var exception = Assert.Throws<ArgumentException>(() =>
            Event.Create(organizerId, "Tech Fest", "Desc", startDate, endDate, "Location", 1000));

        Assert.Contains("End date cannot be earlier", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CreateEvent_CapacityZeroOrLess_ShouldThrowException()
    {
        var startDate = DateTime.UtcNow.AddDays(10);
        var endDate = DateTime.UtcNow.AddDays(12);
        var organizerId = Guid.NewGuid();
        int invalidCapacity = 0;

        Assert.Throws<ArgumentException>(() =>
            Event.Create(organizerId, "Tech Fest", "Desc", startDate, endDate, "Location", invalidCapacity));
    }

    
    [Fact]

    
    public void PublishEvent_ValidConditions_ShouldChangeStatusToPublished()
    {
        var startDate = DateTime.UtcNow.AddDays(10);
        var endDate = DateTime.UtcNow.AddDays(12);
        var salesStart = DateTime.UtcNow;
        var salesEnd = startDate.AddDays(-3);

        var @event = Event.Create(Guid.NewGuid(), "Tech Fest", "Desc", startDate, endDate, "Location", 1000);

        @event.AddTicketCategory("VIP", new Money(150000), 500, salesStart, salesEnd);

        @event.Publish();

        Assert.Equal(EventStatus.Published, @event.Status);
        Assert.Contains(@event.DomainEvents, e => e is EventPublished);
    }


    [Fact]
    public void PublishEvent_NoTicketCategory_ShouldThrowException()
    {
        var @event = Event.Create(Guid.NewGuid(), "Tech Fest", "Desc", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), "Location", 1000);
        
        Assert.Throws<InvalidOperationException>(() => @event.Publish());
    }

    [Fact]
    public void PublishEvent_QuotaExceedsCapacity_ShouldThrowException()
    {
        var @event = Event.Create(Guid.NewGuid(), "Tech Fest", "Desc", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), "Location", 1000);

        Assert.Throws<InvalidOperationException>(() =>
            @event.AddTicketCategory("General", new Money(50000), 1500, DateTime.UtcNow, DateTime.UtcNow.AddDays(7)));
    }

    
    [Fact]

   
    public void CancelEvent_PublishedEvent_ShouldChangeStatusToCancelled()
    {
        var startDate = DateTime.UtcNow.AddDays(10);
        var endDate = DateTime.UtcNow.AddDays(12);
        var salesStart = DateTime.UtcNow;
        var salesEnd = startDate.AddDays(-3);

        var @event = Event.Create(Guid.NewGuid(), "Tech Fest", "Desc", startDate, endDate, "Location", 1000);

        @event.AddTicketCategory("VIP", new Money(150000), 500, salesStart, salesEnd);
        @event.Publish();

        @event.Cancel();

        Assert.Equal(EventStatus.Cancelled, @event.Status);
        Assert.Contains(@event.DomainEvents, e => e is EventCancelled);
    }
}