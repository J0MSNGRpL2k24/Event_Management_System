using EventManagementSystem.Domain.Entities;
using EventManagementSystem.Domain.Events;
using EventManagementSystem.Domain.ValueObjects;
using Xunit;
using System.Linq;

namespace EventManagementSystem.Domain.Tests;

public class BookingTests
{
    private Event SetupValidEventWithCategory(out TicketCategory category)
    {
        var startDate = DateTime.UtcNow.AddDays(10);
        var endDate = DateTime.UtcNow.AddDays(12);

        var @event = Event.Create(Guid.NewGuid(), "Tech Fest", "Desc", startDate, endDate, "Location", 1000);

        @event.AddTicketCategory("VIP", new Money(50000), 100, DateTime.UtcNow, startDate.AddDays(-1));

        category = @event.Categories.First();

        return @event;
    }

    
    [Fact]
    public void CreateBooking_ValidData_ShouldCreateWithPendingPaymentStatus()
    {
        var customerId = Guid.NewGuid();
        var @event = SetupValidEventWithCategory(out var category);
        var quantity = 2;
        decimal serviceFee = 5000m;

        var booking = Booking.Create(customerId, @event, category, quantity, serviceFee);

        Assert.Equal(BookingStatus.PendingPayment, booking.Status);

        var expectedDeadline = DateTime.UtcNow.AddMinutes(15);
        Assert.True(booking.PaymentDeadline <= expectedDeadline && booking.PaymentDeadline >= expectedDeadline.AddMinutes(-1));

        Assert.Contains(booking.DomainEvents, e => e is TicketReserved);
    }

    [Fact]
    public void CreateBooking_QuantityZeroOrLess_ShouldThrowException()
    {
        var customerId = Guid.NewGuid();
        var @event = SetupValidEventWithCategory(out var category);
        int invalidQuantity = 0;
        decimal serviceFee = 5000m;

        var ex = Assert.Throws<ArgumentException>(() =>
            Booking.Create(customerId, @event, category, invalidQuantity, serviceFee));

        Assert.Contains("greater than zero", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    
    [Fact]
    public void CalculateTotalPrice_ValidQuantityAndFee_ShouldReturnCorrectAmount()
    {
        var customerId = Guid.NewGuid();
        var @event = SetupValidEventWithCategory(out var category);
        var quantity = 3;
        decimal serviceFee = 10000m;

        var booking = Booking.Create(customerId, @event, category, quantity, serviceFee);

        var expectedTotal = new Money(160000, category.Price.Currency);
        Assert.Equal(expectedTotal.Amount, booking.TotalPrice.Amount);
    }

    private void SetPaymentDeadlineInPast(Booking booking)
    {
        var propertyInfo = typeof(Booking).GetProperty("PaymentDeadline");
        propertyInfo?.SetValue(booking, DateTime.UtcNow.AddMinutes(-5));
    }

    
    [Fact]
    public void ConfirmPayment_ValidAmountAndWithinDeadline_ShouldChangeStatusToPaidAndIssueTickets()
    {
        var customerId = Guid.NewGuid();
        var @event = SetupValidEventWithCategory(out var category);
        var booking = Booking.Create(customerId, @event, category, 2, 5000m);
        var paymentAmount = new Money(105000, category.Price.Currency);

        booking.ConfirmPayment(paymentAmount);

        Assert.Equal(BookingStatus.Paid, booking.Status);
        Assert.Equal(2, booking.Tickets.Count);
        Assert.Contains(booking.DomainEvents, e => e is BookingPaid);
    }

    [Fact]
    public void ConfirmPayment_WrongAmount_ShouldThrowException()
    {
        var customerId = Guid.NewGuid();
        var @event = SetupValidEventWithCategory(out var category);
        var booking = Booking.Create(customerId, @event, category, 2, 5000m);
        var wrongPaymentAmount = new Money(100000, category.Price.Currency);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            booking.ConfirmPayment(wrongPaymentAmount));
        Assert.Contains("does not match", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ConfirmPayment_DeadlinePassed_ShouldThrowExceptionAndCallExpire()
    {
        var customerId = Guid.NewGuid();
        var @event = SetupValidEventWithCategory(out var category);
        var booking = Booking.Create(customerId, @event, category, 2, 5000m);
        var paymentAmount = new Money(105000, category.Price.Currency);

        SetPaymentDeadlineInPast(booking);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            booking.ConfirmPayment(paymentAmount));
        Assert.Contains("deadline has passed", ex.Message, StringComparison.OrdinalIgnoreCase);

        Assert.Equal(BookingStatus.Expired, booking.Status);
    }

    
    [Fact]
    public void Expire_DeadlinePassed_ShouldChangeStatusToExpiredAndRaiseEvent()
    {
        var customerId = Guid.NewGuid();
        var @event = SetupValidEventWithCategory(out var category);
        var booking = Booking.Create(customerId, @event, category, 2, 5000m);
        SetPaymentDeadlineInPast(booking);

        booking.Expire();

        Assert.Equal(BookingStatus.Expired, booking.Status);
        Assert.Contains(booking.DomainEvents, e => e is BookingExpired);
    }

    [Fact]
    public void Expire_BeforeDeadline_ShouldThrowException()
    {
        var customerId = Guid.NewGuid();
        var @event = SetupValidEventWithCategory(out var category);
        var booking = Booking.Create(customerId, @event, category, 2, 5000m);
        
        var ex = Assert.Throws<InvalidOperationException>(() => booking.Expire());
        Assert.Contains("before its payment deadline", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Expire_AlreadyPaid_ShouldThrowException()
    {
        var customerId = Guid.NewGuid();
        var @event = SetupValidEventWithCategory(out var category);
        var booking = Booking.Create(customerId, @event, category, 2, 5000m);
        booking.ConfirmPayment(new Money(105000, category.Price.Currency));

        var ex = Assert.Throws<InvalidOperationException>(() => booking.Expire());
        Assert.Contains("cannot be expired", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}