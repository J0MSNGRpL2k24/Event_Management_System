using EventManagementSystem.Domain.Entities;
using EventManagementSystem.Domain.Events;
using EventManagementSystem.Domain.ValueObjects;
using Xunit;

namespace EventManagementSystem.Domain.Tests;

public class EventTests
{
    // ==========================================================
    // USER STORY 1: CREATE EVENT
    // ==========================================================

    [Fact]
    public void CreateEvent_ValidData_ShouldCreateEventWithDraftStatus()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(10);
        var endDate = DateTime.UtcNow.AddDays(12);
        var organizerId = Guid.NewGuid();

        // Act
        var @event = Event.Create(organizerId, "Tech Fest", "Annual event", startDate, endDate, "Graha ITS", 1000);

        // Assert
        Assert.Equal(EventStatus.Draft, @event.Status);

        // Memastikan Domain Event EventCreated tercatat
        Assert.Contains(@event.DomainEvents, e => e is EventCreated);
    }

    [Fact]
    public void CreateEvent_EndDateEarlierThanStartDate_ShouldThrowException()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(10);
        var endDate = DateTime.UtcNow.AddDays(5); // Invalid
        var organizerId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Event.Create(organizerId, "Tech Fest", "Desc", startDate, endDate, "Location", 1000));

        Assert.Contains("End date cannot be earlier", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CreateEvent_CapacityZeroOrLess_ShouldThrowException()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(10);
        var endDate = DateTime.UtcNow.AddDays(12);
        var organizerId = Guid.NewGuid();
        int invalidCapacity = 0;

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            Event.Create(organizerId, "Tech Fest", "Desc", startDate, endDate, "Location", invalidCapacity));
    }

    // ==========================================================
    // USER STORY 2: PUBLISH EVENT
    // ==========================================================

    [Fact]

    
    public void PublishEvent_ValidConditions_ShouldChangeStatusToPublished()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(10); // Event mulai 10 hari lagi
        var endDate = DateTime.UtcNow.AddDays(12);
        var salesStart = DateTime.UtcNow;
        var salesEnd = startDate.AddDays(-3); // Tiket ditutup 3 HARI SEBELUM event mulai

        var @event = Event.Create(Guid.NewGuid(), "Tech Fest", "Desc", startDate, endDate, "Location", 1000);

        @event.AddTicketCategory("VIP", new Money(150000), 500, salesStart, salesEnd);

        // Act
        @event.Publish();

        // Assert
        Assert.Equal(EventStatus.Published, @event.Status);
        Assert.Contains(@event.DomainEvents, e => e is EventPublished);
    }


    [Fact]
    public void PublishEvent_NoTicketCategory_ShouldThrowException()
    {
        // Arrange
        var @event = Event.Create(Guid.NewGuid(), "Tech Fest", "Desc", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), "Location", 1000);
        // Sengaja tidak menambahkan tiket kategori

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => @event.Publish());
    }

    [Fact]
    public void PublishEvent_QuotaExceedsCapacity_ShouldThrowException()
    {
        // Arrange
        var @event = Event.Create(Guid.NewGuid(), "Tech Fest", "Desc", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), "Location", 1000);

        // Act & Assert
        // Error akan terpicu langsung saat menambah kategori yang kuotanya melebihi max capacity (1500 > 1000)
        Assert.Throws<InvalidOperationException>(() =>
            @event.AddTicketCategory("General", new Money(50000), 1500, DateTime.UtcNow, DateTime.UtcNow.AddDays(7)));
    }

    // ==========================================================
    // USER STORY 3: CANCEL EVENT
    // ==========================================================

    [Fact]

   
    public void CancelEvent_PublishedEvent_ShouldChangeStatusToCancelled()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(10); // Event mulai 10 hari lagi
        var endDate = DateTime.UtcNow.AddDays(12);
        var salesStart = DateTime.UtcNow;
        var salesEnd = startDate.AddDays(-3); // Tiket ditutup 3 HARI SEBELUM event mulai

        var @event = Event.Create(Guid.NewGuid(), "Tech Fest", "Desc", startDate, endDate, "Location", 1000);

        @event.AddTicketCategory("VIP", new Money(150000), 500, salesStart, salesEnd);
        @event.Publish(); // Status sekarang Published

        // Act
        @event.Cancel();

        // Assert
        Assert.Equal(EventStatus.Cancelled, @event.Status);
        Assert.Contains(@event.DomainEvents, e => e is EventCancelled);
    }
}