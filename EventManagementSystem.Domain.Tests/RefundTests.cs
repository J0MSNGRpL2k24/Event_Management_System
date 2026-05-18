using EventManagementSystem.Domain.Entities;
using EventManagementSystem.Domain.Events;
using EventManagementSystem.Domain.ValueObjects;
using System.Reflection;
using Xunit;
using System.Linq;

namespace EventManagementSystem.Domain.Tests;

public class RefundTests
{
    // ==========================================================
    // HELPER METHODS UNTUK SETUP DATA
    // ==========================================================

    // Membangun Booking yang sudah berstatus PAID dan memiliki tiket
    private Booking SetupPaidBooking(out Event @event)
    {
        var startDate = DateTime.UtcNow.AddDays(10);
        @event = Event.Create(Guid.NewGuid(), "Refund Fest", "Desc", startDate, startDate.AddDays(2), "Loc", 1000);
        @event.AddTicketCategory("VIP", new Money(50000), 10, DateTime.UtcNow, startDate.AddDays(-1));

        var category = @event.Categories.First();
        var booking = Booking.Create(Guid.NewGuid(), @event, category, 2, 5000m); // Total: 105.000

        // Membayar booking agar tiketnya ter-generate dan statusnya Paid
        booking.ConfirmPayment(new Money(105000, category.Price.Currency));
        return booking;
    }

    // Reflection untuk memanipulasi StartDate Event ke masa lalu (Deadline terlewat)
    private void SetEventStartDateInPast(Event @event)
    {
        var prop = typeof(Event).GetProperty("StartDate");
        prop?.SetValue(@event, DateTime.UtcNow.AddDays(-1));
    }

    // Reflection untuk mengubah status event jadi Cancelled
    private void SetEventStatusToCancelled(Event @event)
    {
        var prop = typeof(Event).GetProperty("Status");
        prop?.SetValue(@event, EventStatus.Cancelled);
    }

    //  Reflection untuk mengubah salah satu tiket menjadi CheckedIn
    private void SetTicketStatusToCheckedIn(Ticket ticket)
    {
        var prop = typeof(Ticket).GetProperty("Status");
        prop?.SetValue(ticket, TicketStatus.CheckedIn);
    }

    // ==========================================================
    // USER STORY 15: REQUEST REFUND
    // ==========================================================

    [Fact]
    public void RequestRefund_ValidConditions_ShouldSetStatusToRequestedAndRaiseEvent()
    {
        // Arrange
        var booking = SetupPaidBooking(out var @event);
        var reason = "Mendadak ada urusan keluarga";

        // Act
        var refund = new Refund(booking, @event, reason);

        // Assert
        Assert.Equal(RefundStatus.Requested, refund.Status);
        Assert.Equal(booking.TotalPrice.Amount, refund.Amount.Amount); // Uang kembali = total bayar
        Assert.Contains(refund.DomainEvents, e => e is RefundRequested);
    }

    [Fact]
    public void RequestRefund_BookingNotPaid_ShouldThrowException()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(10);
        var @event = Event.Create(Guid.NewGuid(), "Fest", "Desc", startDate, startDate.AddDays(2), "Loc", 100);
        @event.AddTicketCategory("VIP", new Money(50000), 10, DateTime.UtcNow, startDate.AddDays(-1));

        // Booking dibuat tapi BELUM dibayar (Status masih PendingPayment)
        var booking = Booking.Create(Guid.NewGuid(), @event, @event.Categories.First(), 1, 5000m);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => new Refund(booking, @event, "Batal"));
        Assert.Contains("paid bookings", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RequestRefund_TicketAlreadyCheckedIn_ShouldThrowException()
    {
        // Arrange
        var booking = SetupPaidBooking(out var @event);
        SetTicketStatusToCheckedIn(booking.Tickets.First()); // Paksa 1 tiket jadi CheckedIn

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => new Refund(booking, @event, "Batal"));
        Assert.Contains("checked in", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RequestRefund_DeadlinePassed_ShouldThrowException()
    {
        // Arrange
        var booking = SetupPaidBooking(out var @event);
        SetEventStartDateInPast(@event); // Paksa waktu event jadi kemarin (Deadline habis)

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => new Refund(booking, @event, "Telat batal"));
        Assert.Contains("deadline has passed", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RequestRefund_EventCancelledButDeadlinePassed_ShouldBypassDeadlineAndSucceed()
    {
        // Arrange
        var booking = SetupPaidBooking(out var @event);
        SetEventStartDateInPast(@event); // Waktu event sudah lewat
        SetEventStatusToCancelled(@event); //  event yang dibatalkan oleh panitia

        // Act
        var refund = new Refund(booking, @event, "Event batal");

        // Assert
        // Harus sukses tanpa error karena Event Cancelled mengabaikan deadline
        Assert.Equal(RefundStatus.Requested, refund.Status);
    }

    // ==========================================================
    // USER STORY 16: APPROVE REFUND
    // ==========================================================

    [Fact]
    public void ApproveRefund_RequestedStatus_ShouldChangeToApprovedAndRaiseEvent()
    {
        // Arrange
        var booking = SetupPaidBooking(out var @event);
        var refund = new Refund(booking, @event, "Batal");

        // Act
        refund.Approve();

        // Assert
        Assert.Equal(RefundStatus.Approved, refund.Status);
        Assert.Contains(refund.DomainEvents, e => e is RefundApproved);
    }

    // ==========================================================
    // USER STORY 17: REJECT REFUND
    // ==========================================================

    [Fact]
    public void RejectRefund_ValidReason_ShouldChangeToRejectedAndRaiseEvent()
    {
        // Arrange
        var booking = SetupPaidBooking(out var @event);
        var refund = new Refund(booking, @event, "Batal");
        var rejectionReason = "Alasan tidak valid sesuai S&K";

        // Act
        refund.Reject(rejectionReason);

        // Assert
        Assert.Equal(RefundStatus.Rejected, refund.Status);
        Assert.Equal(rejectionReason, refund.RejectionReason);
        Assert.Contains(refund.DomainEvents, e => e is RefundRejected);
    }

    [Fact]
    public void RejectRefund_EmptyReason_ShouldThrowException()
    {
        // Arrange
        var booking = SetupPaidBooking(out var @event);
        var refund = new Refund(booking, @event, "Batal");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => refund.Reject(""));
    }

    // ==========================================================
    // USER STORY 18: MARK REFUND AS PAID OUT
    // ==========================================================

    [Fact]
    public void MarkAsPaidOut_ApprovedRefund_ShouldChangeToPaidOutAndRaiseEvent()
    {
        // Arrange
        var booking = SetupPaidBooking(out var @event);
        var refund = new Refund(booking, @event, "Batal");
        refund.Approve(); // Harus di-approve terlebih dahulu agar bisa MarkAsPaidOut
        var refCode = "TRX-BANK-001";

        // Act
        refund.MarkAsPaidOut(refCode);

        // Assert
        Assert.Equal(RefundStatus.PaidOut, refund.Status);
        Assert.Equal(refCode, refund.PaymentReference);
        Assert.Contains(refund.DomainEvents, e => e is RefundPaidOut);
    }

    [Fact]
    public void MarkAsPaidOut_NotApproved_ShouldThrowException()
    {
        // Arrange
        var booking = SetupPaidBooking(out var @event);
        var refund = new Refund(booking, @event, "Batal");
        //  TIDAK di-approve (Status masih Requested)

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => refund.MarkAsPaidOut("TRX-001"));
        Assert.Contains("Only approved refunds", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}