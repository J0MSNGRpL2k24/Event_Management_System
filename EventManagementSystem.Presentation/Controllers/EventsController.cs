using EventManagementSystem.Application.Commands.CancelEvent;
using EventManagementSystem.Application.Commands.CreateBooking;
using EventManagementSystem.Application.Commands.CreateEvent;
using EventManagementSystem.Application.Commands.CreateTicketCategory;
using EventManagementSystem.Application.Commands.DisableTicketCategory;
using EventManagementSystem.Application.Commands.PublishEvent;
using EventManagementSystem.Application.Queries.GetAvailableEvents;
using EventManagementSystem.Application.Queries.GetEventDetail;

using MediatR;
using Microsoft.AspNetCore.Mvc;

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
       
        var eventId = await _mediator.Send(command);
        return Ok(new { Id = eventId, Message = "Event created successfully." });
    }


    [HttpPost("{id}/categories")]
    public async Task<IActionResult> AddCategory(Guid id, [FromBody] CreateTicketCategoryCommand command)
    {
        
        if (id != command.EventId) return BadRequest("Event ID mismatch.");

        var categoryId = await _mediator.Send(command);
        return Ok(new { Id = categoryId, Message = "Ticket category created successfully." });
    }

    [HttpPost("{id}/publish")]
    public async Task<IActionResult> PublishEvent(Guid id)
    {
        try
        {
            await _mediator.Send(new PublishEventCommand(id));
            return Ok(new { Message = "Event published successfully." });
        }
        catch (InvalidOperationException ex)
        {
            
            return BadRequest(new { Message = ex.Message });
        }
    }


    [HttpPost("bookings")]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingCommand command)
    {
        var bookingId = await _mediator.Send(command);
        return Ok(new { Id = bookingId, Message = "Tickets reserved! Please pay within 15 minutes." });
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelEvent(Guid id)
    {
        await _mediator.Send(new CancelEventCommand(id));
        return Ok(new { Message = "Event has been cancelled. All paid bookings are marked for refund." });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetail(Guid id)
    {
        var result = await _mediator.Send(new GetEventDetailQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPatch("{eventId}/categories/{categoryId}/disable")]
    public async Task<IActionResult> DisableCategory(Guid eventId, Guid categoryId)
    {
        await _mediator.Send(new DisableTicketCategoryCommand(eventId, categoryId));
        return Ok(new { Message = "Ticket category has been disabled." });
    }

}