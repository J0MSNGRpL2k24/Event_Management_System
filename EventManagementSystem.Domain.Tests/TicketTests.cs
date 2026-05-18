using EventManagementSystem.Domain.Entities;
using EventManagementSystem.Domain.Events;
using System.Reflection;
using Xunit;

namespace EventManagementSystem.Domain.Tests;

public class TicketTests
{
    // ==========================================================
    // HELPER METHODS
    // ==========================================================

    // Helper: Membuat Event yang acaranya HARI INI (agar lolos validasi hari check-in)
    private Event CreateEventForToday()
    {
        var startDate = DateTime.UtcNow; // Event hari ini
        var endDate = DateTime.UtcNow.AddDays(1);

        return Event.Create(Guid.NewGuid(), "Tech Fest", "Desc", startDate, endDate, "Location", 1000);
    }

    // Helper: Membuat Event di masa depan (untuk memicu error hari check-in)
    private Event CreateEventForFuture()
    {
        var startDate = DateTime.UtcNow.AddDays(5); // Event masih 5 hari lagi
        var endDate = DateTime.UtcNow.AddDays(6);

        return Event.Create(Guid.NewGuid(), "Future Fest", "Desc", startDate, endDate, "Location", 1000);
    }

    // Helper: Menggunakan Reflection untuk membuat objek Ticket karena constructor-nya internal
    private Ticket CreateTicket(Guid eventId)
    {
        var bookingId = Guid.NewGuid();
        var ticketCode = $"TIX-{bookingId.ToString().Substring(0, 8).ToUpper()}-1";

        // Memanggil: internal Ticket(Guid bookingId, Guid eventId, string ticketCode)
        var ticket = (Ticket)Activator.CreateInstance(
            typeof(Ticket),
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            new object[] { bookingId, eventId, ticketCode },
            null)!;

        return ticket;
    }

    // ==========================================================
    // USER STORY 13: CHECK IN TICKET
    // ==========================================================

    [Fact]
    public void CheckIn_ValidConditions_ShouldChangeStatusToCheckedInAndRaiseEvent()
    {
        // Arrange
        var @event = CreateEventForToday();
        var ticket = CreateTicket(@event.Id);

        // Act
        ticket.CheckIn(@event);

        // Assert
        Assert.Equal(TicketStatus.CheckedIn, ticket.Status);
        Assert.Contains(ticket.DomainEvents, e => e is TicketCheckedIn);
    }

    // ==========================================================
    // USER STORY 14: REJECT INVALID TICKET CHECK-IN
    // ==========================================================

    [Fact]
    public void CheckIn_EventCancelled_ShouldThrowExceptionAndNotChangeStatus()
    {
        // Arrange
        var @event = CreateEventForToday();
        // Paksa status event menjadi Draft dulu, Publish, lalu Cancel 
        
        // Asumsi: @event status bisa diubah atau kita buat event khusus yang statusnya Cancelled
        var propertyInfo = typeof(Event).GetProperty("Status");
        propertyInfo?.SetValue(@event, EventStatus.Cancelled); 

        var ticket = CreateTicket(@event.Id);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => ticket.CheckIn(@event));
        Assert.Contains("cancelled", ex.Message, StringComparison.OrdinalIgnoreCase);

        // Validasi AC: "The ticket status must not change if check-in fails"
        Assert.Equal(TicketStatus.Active, ticket.Status);
    }

    [Fact]
    public void CheckIn_DifferentEvent_ShouldThrowException()
    {
        // Arrange
        var @event = CreateEventForToday();
        var differentEventId = Guid.NewGuid(); // ID Event yang berbeda
        var ticket = CreateTicket(differentEventId); // Tiket dibuat untuk event yang salah

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => ticket.CheckIn(@event));
        Assert.Contains("does not match", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(TicketStatus.Active, ticket.Status);
    }

    [Fact]
    public void CheckIn_AlreadyCheckedIn_ShouldThrowException()
    {
        // Arrange
        var @event = CreateEventForToday();
        var ticket = CreateTicket(@event.Id);

        // Melakukan check-in pertama yang sukses
        ticket.CheckIn(@event);

        // Act & Assert (Mencoba check-in tiket yang sama untuk kedua kalinya)
        var ex = Assert.Throws<InvalidOperationException>(() => ticket.CheckIn(@event));
        Assert.Contains("already been used", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CheckIn_NotOnEventDay_ShouldThrowException()
    {
        // Arrange
        var futureEvent = CreateEventForFuture(); // Eventnya minggu depan
        var ticket = CreateTicket(futureEvent.Id);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => ticket.CheckIn(futureEvent));
        Assert.Contains("on the event day", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(TicketStatus.Active, ticket.Status);
    }
}