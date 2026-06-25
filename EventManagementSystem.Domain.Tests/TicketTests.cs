using EventManagementSystem.Domain.Entities;
using EventManagementSystem.Domain.Events;
using System.Reflection;
using Xunit;

namespace EventManagementSystem.Domain.Tests;

public class TicketTests
{

    private Event CreateEventForToday()
    {
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddDays(1);

        return Event.Create(Guid.NewGuid(), "Tech Fest", "Desc", startDate, endDate, "Location", 1000);
    }

    private Event CreateEventForFuture()
    {
        var startDate = DateTime.UtcNow.AddDays(5);
        var endDate = DateTime.UtcNow.AddDays(6);

        return Event.Create(Guid.NewGuid(), "Future Fest", "Desc", startDate, endDate, "Location", 1000);
    }

    private Ticket CreateTicket(Guid eventId)
    {
        var bookingId = Guid.NewGuid();
        var ticketCode = $"TIX-{bookingId.ToString().Substring(0, 8).ToUpper()}-1";

        var ticket = (Ticket)Activator.CreateInstance(
            typeof(Ticket),
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            new object[] { bookingId, eventId, ticketCode },
            null)!;

        return ticket;
    }


    [Fact]
    public void CheckIn_ValidConditions_ShouldChangeStatusToCheckedInAndRaiseEvent()
    {
        var @event = CreateEventForToday();
        var ticket = CreateTicket(@event.Id);

        ticket.CheckIn(@event);

        Assert.Equal(TicketStatus.CheckedIn, ticket.Status);
        Assert.Contains(ticket.DomainEvents, e => e is TicketCheckedIn);
    }


    [Fact]
    public void CheckIn_EventCancelled_ShouldThrowExceptionAndNotChangeStatus()
    {
        var @event = CreateEventForToday();
        
        var propertyInfo = typeof(Event).GetProperty("Status");
        propertyInfo?.SetValue(@event, EventStatus.Cancelled); 

        var ticket = CreateTicket(@event.Id);

        var ex = Assert.Throws<InvalidOperationException>(() => ticket.CheckIn(@event));
        Assert.Contains("cancelled", ex.Message, StringComparison.OrdinalIgnoreCase);

        Assert.Equal(TicketStatus.Active, ticket.Status);
    }

    [Fact]
    public void CheckIn_DifferentEvent_ShouldThrowException()
    {
        var @event = CreateEventForToday();
        var differentEventId = Guid.NewGuid();
        var ticket = CreateTicket(differentEventId);

        var ex = Assert.Throws<InvalidOperationException>(() => ticket.CheckIn(@event));
        Assert.Contains("does not match", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(TicketStatus.Active, ticket.Status);
    }

    [Fact]
    public void CheckIn_AlreadyCheckedIn_ShouldThrowException()
    {
        var @event = CreateEventForToday();
        var ticket = CreateTicket(@event.Id);

        ticket.CheckIn(@event);

        var ex = Assert.Throws<InvalidOperationException>(() => ticket.CheckIn(@event));
        Assert.Contains("already been used", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CheckIn_NotOnEventDay_ShouldThrowException()
    {
        var futureEvent = CreateEventForFuture();
        var ticket = CreateTicket(futureEvent.Id);

        var ex = Assert.Throws<InvalidOperationException>(() => ticket.CheckIn(futureEvent));
        Assert.Contains("on the event day", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(TicketStatus.Active, ticket.Status);
    }
}