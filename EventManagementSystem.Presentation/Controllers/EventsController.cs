using EventManagementSystem.Application.Commands.CancelEvent;
using EventManagementSystem.Application.Commands.CreateEvent;
using EventManagementSystem.Application.Commands.CreateTicketCategory;
using EventManagementSystem.Application.Commands.DisableTicketCategory;
using EventManagementSystem.Application.Commands.PublishEvent;
using EventManagementSystem.Application.Queries.GetAvailableEvents;
using EventManagementSystem.Application.Queries.GetEventDetail;
using EventManagementSystem.Application.Queries.ViewEventParticipants;
using EventManagementSystem.Application.Queries.ViewEventSalesReport;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventManagementSystem.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IMediator _mediator;

    public EventsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventCommand command)
    {
        try
        {
            var eventId = await _mediator.Send(command);
            return Ok(new { Id = eventId, Message = "Event created successfully." });
        }
        catch (Exception ex)
        {
            // Menangkap error jika tanggal terbalik atau kapasitas <= 0
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("{id}/publish")]
    public async Task<IActionResult> PublishEvent(Guid id)
    {
        try
        {
            await _mediator.Send(new PublishEventCommand(id));
            return Ok(new { Message = "Event published successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelEvent(Guid id)
    {
        try
        {
            await _mediator.Send(new CancelEventCommand(id));
            return Ok(new { Message = "Event has been cancelled. All paid bookings are marked for refund." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    // --- Section Ticket Category (Sudah bagus, kita pertahankan dulu) ---

    [HttpPost("{id}/categories")]
    public async Task<IActionResult> AddCategory(Guid id, [FromBody] CreateTicketCategoryCommand command)
    {
        try
        {
            if (id != command.EventId) return BadRequest(new { Message = "Event ID mismatch." });

            var categoryId = await _mediator.Send(command);
            return Ok(new { Id = categoryId, Message = "Ticket category created successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPatch("{eventId}/categories/{categoryId}/disable")]
    public async Task<IActionResult> DisableCategory(Guid eventId, Guid categoryId)
    {
        try
        {
            await _mediator.Send(new DisableTicketCategoryCommand(eventId, categoryId));
            return Ok(new { Message = "Ticket category has been disabled." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    // --- Section Queries ---

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetail(Guid id)
    {
        var result = await _mediator.Send(new GetEventDetailQuery(id));
        if (result == null) return NotFound(new { Message = "Event not found." });
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailableEvents([FromQuery] DateTime? date, [FromQuery] string? location)
    {
        try
        {
            var query = new GetAvailableEventsQuery(date, location);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }


    // --- USER STORY 19: View Event Sales Report ---
    [HttpGet("{id}/report")]
    public async Task<IActionResult> GetSalesReport(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new ViewEventSalesReportQuery(id));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    // --- USER STORY 20: View Event Participants ---
    [HttpGet("{id}/participants")]
    public async Task<IActionResult> GetParticipants(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new ViewEventParticipantsQuery(id));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}