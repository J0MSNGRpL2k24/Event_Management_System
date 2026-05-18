using EventManagementSystem.Domain.Entities;
using EventManagementSystem.Domain.Events;
using EventManagementSystem.Domain.ValueObjects;
using Xunit;
using System.Linq;

namespace EventManagementSystem.Domain.Tests;

public class BookingTests
{
    // ==========================================================
    // HELPER METHOD: Menyiapkan Event dan Category yang Valid
    // ==========================================================
    private Event SetupValidEventWithCategory(out TicketCategory category)
    {
        var startDate = DateTime.UtcNow.AddDays(10);
        var endDate = DateTime.UtcNow.AddDays(12);

        var @event = Event.Create(Guid.NewGuid(), "Tech Fest", "Desc", startDate, endDate, "Location", 1000);

        // Harga tiket dibuat 50.000 untuk mempermudah hitungan tes US 9
        @event.AddTicketCategory("VIP", new Money(50000), 100, DateTime.UtcNow, startDate.AddDays(-1));

        // Mengambil objek kategori yang baru saja dibuat untuk dimasukkan ke Booking
        category = @event.Categories.First();

        return @event;
    }

    // ==========================================================
    // USER STORY 8: CREATE TICKET BOOKING
    // ==========================================================

    [Fact]
    public void CreateBooking_ValidData_ShouldCreateWithPendingPaymentStatus()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var @event = SetupValidEventWithCategory(out var category);
        var quantity = 2;
        decimal serviceFee = 5000m;

        // Act
        // Parameter sesuai dengan Booking.cs: customerId, Event, TicketCategory, quantity, serviceFee
        var booking = Booking.Create(customerId, @event, category, quantity, serviceFee);

        // Assert
        Assert.Equal(BookingStatus.PendingPayment, booking.Status);

        // Validasi waktu deadline 15 menit
        var expectedDeadline = DateTime.UtcNow.AddMinutes(15);
        Assert.True(booking.PaymentDeadline <= expectedDeadline && booking.PaymentDeadline >= expectedDeadline.AddMinutes(-1));

        // Validasi Domain Event
        Assert.Contains(booking.DomainEvents, e => e is TicketReserved);
    }

    [Fact]
    public void CreateBooking_QuantityZeroOrLess_ShouldThrowException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var @event = SetupValidEventWithCategory(out var category);
        int invalidQuantity = 0;
        decimal serviceFee = 5000m;

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            Booking.Create(customerId, @event, category, invalidQuantity, serviceFee));

        Assert.Contains("greater than zero", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ==========================================================
    // USER STORY 9: CALCULATE BOOKING TOTAL PRICE
    // ==========================================================

    [Fact]
    public void CalculateTotalPrice_ValidQuantityAndFee_ShouldReturnCorrectAmount()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var @event = SetupValidEventWithCategory(out var category); // Harga tiket base = 50.000
        var quantity = 3;
        decimal serviceFee = 10000m;

        // Act
        var booking = Booking.Create(customerId, @event, category, quantity, serviceFee);

        // Assert
        // Rumus: (Harga 50.000 * 3 tiket) + Service Fee 10.000 = 160.000
        var expectedTotal = new Money(160000, category.Price.Currency);
        Assert.Equal(expectedTotal.Amount, booking.TotalPrice.Amount);
    }

    // Helper Method: Memanipulasi waktu secara paksa menggunakan Reflection untuk simulasi Expired
    private void SetPaymentDeadlineInPast(Booking booking)
    {
        var propertyInfo = typeof(Booking).GetProperty("PaymentDeadline");
        propertyInfo?.SetValue(booking, DateTime.UtcNow.AddMinutes(-5));
    }

    // ==========================================================
    // USER STORY 10: PAY BOOKING
    // ==========================================================

    [Fact]
    public void ConfirmPayment_ValidAmountAndWithinDeadline_ShouldChangeStatusToPaidAndIssueTickets()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var @event = SetupValidEventWithCategory(out var category);
        var booking = Booking.Create(customerId, @event, category, 2, 5000m); // Total 105.000
        var paymentAmount = new Money(105000, category.Price.Currency);

        // Act
        booking.ConfirmPayment(paymentAmount);

        // Assert
        Assert.Equal(BookingStatus.Paid, booking.Status);
        Assert.Equal(2, booking.Tickets.Count); // Memastikan 2 tiket degenerate
        Assert.Contains(booking.DomainEvents, e => e is BookingPaid);
    }

    [Fact]
    public void ConfirmPayment_WrongAmount_ShouldThrowException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var @event = SetupValidEventWithCategory(out var category);
        var booking = Booking.Create(customerId, @event, category, 2, 5000m); // Total 105.000
        var wrongPaymentAmount = new Money(100000, category.Price.Currency); // Kurang bayar

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            booking.ConfirmPayment(wrongPaymentAmount));
        Assert.Contains("does not match", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ConfirmPayment_DeadlinePassed_ShouldThrowExceptionAndCallExpire()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var @event = SetupValidEventWithCategory(out var category);
        var booking = Booking.Create(customerId, @event, category, 2, 5000m);
        var paymentAmount = new Money(105000, category.Price.Currency);

        // Memanipulasi waktu deadline agar seolah-olah sudah lewat 5 menit yang lalu
        SetPaymentDeadlineInPast(booking);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            booking.ConfirmPayment(paymentAmount));
        Assert.Contains("deadline has passed", ex.Message, StringComparison.OrdinalIgnoreCase);

        // Validasi bahwa Expire() otomatis terpanggil (Status berubah jadi Expired)
        Assert.Equal(BookingStatus.Expired, booking.Status);
    }

    // ==========================================================
    // USER STORY 11: EXPIRE BOOKING
    // ==========================================================

    [Fact]
    public void Expire_DeadlinePassed_ShouldChangeStatusToExpiredAndRaiseEvent()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var @event = SetupValidEventWithCategory(out var category);
        var booking = Booking.Create(customerId, @event, category, 2, 5000m);
        SetPaymentDeadlineInPast(booking); // Waktu di-set ke masa lalu

        // Act
        booking.Expire();

        // Assert
        Assert.Equal(BookingStatus.Expired, booking.Status);
        Assert.Contains(booking.DomainEvents, e => e is BookingExpired);
    }

    [Fact]
    public void Expire_BeforeDeadline_ShouldThrowException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var @event = SetupValidEventWithCategory(out var category);
        var booking = Booking.Create(customerId, @event, category, 2, 5000m);
        // Deadline belum lewat (masih 15 menit lagi)

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => booking.Expire());
        Assert.Contains("before its payment deadline", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Expire_AlreadyPaid_ShouldThrowException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var @event = SetupValidEventWithCategory(out var category);
        var booking = Booking.Create(customerId, @event, category, 2, 5000m);
        booking.ConfirmPayment(new Money(105000, category.Price.Currency)); // Status jadi Paid

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => booking.Expire());
        Assert.Contains("cannot be expired", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}