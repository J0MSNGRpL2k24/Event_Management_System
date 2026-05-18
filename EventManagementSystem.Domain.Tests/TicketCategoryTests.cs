using EventManagementSystem.Domain.Entities;
using EventManagementSystem.Domain.Events;
using EventManagementSystem.Domain.ValueObjects;
using Xunit;
using System.Linq;

namespace EventManagementSystem.Domain.Tests;

public class TicketCategoryTests
{
    // Helper method: Membuat event dasar yang valid agar kode Arrange lebih bersih
    private Event CreateValidEvent()
    {
        var organizerId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(10);
        var endDate = DateTime.UtcNow.AddDays(12);
        
        return Event.Create(organizerId, "Tech Fest", "Desc", startDate, endDate, "Graha ITS", 1000);
    }

    // ==========================================================
    // USER STORY 4: CREATE TICKET CATEGORY
    // ==========================================================

    [Fact]
    public void AddTicketCategory_ValidData_ShouldCreateCategoryAndRaiseEvent()
    {
        // Arrange
        var @event = CreateValidEvent();
        var salesStart = DateTime.UtcNow;
        var salesEnd = @event.StartDate.AddDays(-1); // Sah: Berakhir sebelum event dimulai

        // Act
        @event.AddTicketCategory("VIP", new Money(150000), 500, salesStart, salesEnd);

        // Assert
        Assert.Single(@event.Categories); // Memastikan ada tepat 1 kategori yang masuk
        Assert.Contains(@event.DomainEvents, e => e is TicketCategoryCreated);
    }

    [Fact]
    public void AddTicketCategory_QuotaZeroOrLess_ShouldThrowException()
    {
        // Arrange
        var @event = CreateValidEvent();

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => 
            @event.AddTicketCategory("VIP", new Money(150000), 0, DateTime.UtcNow, @event.StartDate.AddDays(-1)));
        
        Assert.Contains("greater than zero", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AddTicketCategory_SalesEndAfterEventStart_ShouldThrowException()
    {
        // Arrange
        var @event = CreateValidEvent();
        var salesStart = DateTime.UtcNow;
        var salesEnd = @event.StartDate.AddDays(1); // Invalid: Periode jual berakhir setelah event mulai

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => 
            @event.AddTicketCategory("VIP", new Money(150000), 500, salesStart, salesEnd));
        
        Assert.Contains("before the event starts", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AddTicketCategory_TotalQuotaExceedsMaxCapacity_ShouldThrowException()
    {
        // Arrange
        var @event = CreateValidEvent();
        
        // Act & Assert
        // Event memiliki max capacity 1000. Memasukkan kuota 1500 harus gagal.
        var ex = Assert.Throws<InvalidOperationException>(() => 
            @event.AddTicketCategory("VIP", new Money(150000), 1500, DateTime.UtcNow, @event.StartDate.AddDays(-1)));
        
        Assert.Contains("exceeds event capacity", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ==========================================================
    // USER STORY 5: DISABLE TICKET CATEGORY
    // ==========================================================

    [Fact]
    public void DisableTicketCategory_ValidCategory_ShouldDeactivateAndRaiseEvent()
    {
        // Arrange
        var @event = CreateValidEvent();
        @event.AddTicketCategory("VIP", new Money(150000), 500, DateTime.UtcNow, @event.StartDate.AddDays(-1));
        var categoryId = @event.Categories.First().Id;

        // Act
        @event.DisableTicketCategory(categoryId);

        // Assert
        var category = @event.Categories.First(c => c.Id == categoryId);
        Assert.False(category.IsActive); // Memvalidasi status aktif sudah menjadi false
        Assert.Contains(@event.DomainEvents, e => e is TicketCategoryDisabled);
    }

    [Fact]
    public void DisableTicketCategory_CategoryNotFound_ShouldThrowException()
    {
        // Arrange
        var @event = CreateValidEvent();
        var randomCategoryId = Guid.NewGuid(); // ID sembarang yang tidak ada di kategori manapun dalam event ini

        // Act & Assert
        var ex = Assert.Throws<Exception>(() => 
            @event.DisableTicketCategory(randomCategoryId));
        
        Assert.Contains("not found", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}