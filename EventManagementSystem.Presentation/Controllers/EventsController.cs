using EventManagementSystem.Application.Commands.CancelEvent;
using EventManagementSystem.Application.Commands.CreateEvent;
using EventManagementSystem.Application.Commands.CreateTicketCategory;
using EventManagementSystem.Application.Commands.PublishEvent;
using EventManagementSystem.Application.Queries.GetAvailableEvents;
using EventManagementSystem.Application.Queries.GetEventById;
using EventManagementSystem.Application.Queries.GetEventDetail;
using EventManagementSystem.Application.Queries.ViewEventParticipants;
using EventManagementSystem.Application.Queries.ViewEventSalesReport;
using EventManagementSystem.Domain.Entities;
using EventManagementSystem.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly AppDbContext _context;

    public EventsController(IMediator mediator, AppDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEventCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("{id}/publish")]
    public async Task<IActionResult> Publish(Guid id)
    {
        var command = new PublishEventCommand(id);
        await _mediator.Send(command);
        return Ok();
    }

    [HttpPost("{id}/categories")]
    public async Task<IActionResult> AddCategory(Guid id, [FromBody] CreateTicketCategoryCommand command)
    {
        if (id != command.EventId)
        {
            return BadRequest("Event ID mismatch.");
        }

        var categoryId = await _mediator.Send(command);
        return Ok(categoryId);
    }
   


    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var command = new CancelEventCommand(id);
        await _mediator.Send(command);

        return NoContent();
    }
    [HttpPost("{id}/complete")]
    public async Task<IActionResult> Complete(Guid id)
    {
        var @event = await _context.Events.FindAsync(id);
        if (@event == null) return NotFound();

        @event.Complete();
        await _context.SaveChangesAsync();
        return Ok("Status: Completed");
    }

    [HttpPost("{id}/force-publish")]
    public async Task<IActionResult> ForcePublish(Guid id)
    {
        var @event = await _context.Events.FindAsync(id);
        if (@event == null) return NotFound();

        @event.ResetToPublishedForTest();
        await _context.SaveChangesAsync();
        return Ok("Status reset to Published (1)");
    }
    
    
    

    [HttpGet]
    public async Task<IActionResult> GetAvailableEvents([FromQuery] DateTime? filterDate, [FromQuery] string? filterLocation)
    {
        try
        {
            var query = new GetAvailableEventsQuery(filterDate, filterLocation);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEventDetails(Guid id)
    {
        try
        {
            var query = new GetEventDetailQuery(id);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(new { Message = $"Event with ID {id} not found." });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }


    [HttpGet("{id}/sales-report")]
    public async Task<IActionResult> GetSalesReport(Guid id)
    {
        try
        {
            var query = new ViewEventSalesReportQuery(id);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("{id}/participants")]
    public async Task<IActionResult> GetParticipants(Guid id)
    {
        try
        {
            var query = new ViewEventParticipantsQuery(id);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

}