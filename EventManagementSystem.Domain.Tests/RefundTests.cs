using EventManagementSystem.Domain.Entities;
using EventManagementSystem.Domain.Events;
using EventManagementSystem.Domain.ValueObjects;
using System.Reflection;
using Xunit;
using System.Linq;

namespace EventManagementSystem.Domain.Tests;

public class RefundTests
{
    
    private Booking SetupPaidBooking(out Event @event)
    {
        var startDate = DateTime.UtcNow.AddDays(10);
        @event = Event.Create(Guid.NewGuid(), "Refund Fest", "Desc", startDate, startDate.AddDays(2), "Loc", 1000);
        @event.AddTicketCategory("VIP", new Money(50000), 10, DateTime.UtcNow, startDate.AddDays(-1));

        var category = @event.Categories.First();
        var booking = Booking.Create(Guid.NewGuid(), @event, category, 2, 5000m);

        booking.ConfirmPayment(new Money(105000, category.Price.Currency));
        return booking;
    }

    private void SetEventStartDateInPast(Event @event)
    {
        var prop = typeof(Event).GetProperty("StartDate");
        prop?.SetValue(@event, DateTime.UtcNow.AddDays(-1));
    }

    private void SetEventStatusToCancelled(Event @event)
    {
        var prop = typeof(Event).GetProperty("Status");
        prop?.SetValue(@event, EventStatus.Cancelled);
    }

    private void SetTicketStatusToCheckedIn(Ticket ticket)
    {
        var prop = typeof(Ticket).GetProperty("Status");
        prop?.SetValue(ticket, TicketStatus.CheckedIn);
    }


    [Fact]
    public void RequestRefund_ValidConditions_ShouldSetStatusToRequestedAndRaiseEvent()
    {
        var booking = SetupPaidBooking(out var @event);
        var reason = "Mendadak ada urusan keluarga";

        var refund = new Refund(booking, @event, reason);

        Assert.Equal(RefundStatus.Requested, refund.Status);
        Assert.Equal(booking.TotalPrice.Amount, refund.Amount.Amount);
        Assert.Contains(refund.DomainEvents, e => e is RefundRequested);
    }

    [Fact]
    public void RequestRefund_BookingNotPaid_ShouldThrowException()
    {
        var startDate = DateTime.UtcNow.AddDays(10);
        var @event = Event.Create(Guid.NewGuid(), "Fest", "Desc", startDate, startDate.AddDays(2), "Loc", 100);
        @event.AddTicketCategory("VIP", new Money(50000), 10, DateTime.UtcNow, startDate.AddDays(-1));

        var booking = Booking.Create(Guid.NewGuid(), @event, @event.Categories.First(), 1, 5000m);

        var ex = Assert.Throws<InvalidOperationException>(() => new Refund(booking, @event, "Batal"));
        Assert.Contains("paid bookings", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RequestRefund_TicketAlreadyCheckedIn_ShouldThrowException()
    {
        var booking = SetupPaidBooking(out var @event);
        SetTicketStatusToCheckedIn(booking.Tickets.First());

        var ex = Assert.Throws<InvalidOperationException>(() => new Refund(booking, @event, "Batal"));
        Assert.Contains("checked in", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RequestRefund_DeadlinePassed_ShouldThrowException()
    {
        var booking = SetupPaidBooking(out var @event);
        SetEventStartDateInPast(@event);

        var ex = Assert.Throws<InvalidOperationException>(() => new Refund(booking, @event, "Telat batal"));
        Assert.Contains("deadline has passed", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RequestRefund_EventCancelledButDeadlinePassed_ShouldBypassDeadlineAndSucceed()
    {
        var booking = SetupPaidBooking(out var @event);
        SetEventStartDateInPast(@event);
        SetEventStatusToCancelled(@event);

        var refund = new Refund(booking, @event, "Event batal");

        Assert.Equal(RefundStatus.Requested, refund.Status);
    }


    [Fact]
    public void ApproveRefund_RequestedStatus_ShouldChangeToApprovedAndRaiseEvent()
    {
        var booking = SetupPaidBooking(out var @event);
        var refund = new Refund(booking, @event, "Batal");

        refund.Approve();

        Assert.Equal(RefundStatus.Approved, refund.Status);
        Assert.Contains(refund.DomainEvents, e => e is RefundApproved);
    }


    [Fact]
    public void RejectRefund_ValidReason_ShouldChangeToRejectedAndRaiseEvent()
    {
        var booking = SetupPaidBooking(out var @event);
        var refund = new Refund(booking, @event, "Batal");
        var rejectionReason = "Alasan tidak valid sesuai S&K";

        refund.Reject(rejectionReason);

        Assert.Equal(RefundStatus.Rejected, refund.Status);
        Assert.Equal(rejectionReason, refund.RejectionReason);
        Assert.Contains(refund.DomainEvents, e => e is RefundRejected);
    }

    [Fact]
    public void RejectRefund_EmptyReason_ShouldThrowException()
    {
        var booking = SetupPaidBooking(out var @event);
        var refund = new Refund(booking, @event, "Batal");

        Assert.Throws<ArgumentException>(() => refund.Reject(""));
    }


    [Fact]
    public void MarkAsPaidOut_ApprovedRefund_ShouldChangeToPaidOutAndRaiseEvent()
    {
        var booking = SetupPaidBooking(out var @event);
        var refund = new Refund(booking, @event, "Batal");
        refund.Approve();
        var refCode = "TRX-BANK-001";

        refund.MarkAsPaidOut(refCode);

        Assert.Equal(RefundStatus.PaidOut, refund.Status);
        Assert.Equal(refCode, refund.PaymentReference);
        Assert.Contains(refund.DomainEvents, e => e is RefundPaidOut);
    }

    [Fact]
    public void MarkAsPaidOut_NotApproved_ShouldThrowException()
    {
        var booking = SetupPaidBooking(out var @event);
        var refund = new Refund(booking, @event, "Batal");

        var ex = Assert.Throws<InvalidOperationException>(() => refund.MarkAsPaidOut("TRX-001"));
        Assert.Contains("Only approved refunds", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}