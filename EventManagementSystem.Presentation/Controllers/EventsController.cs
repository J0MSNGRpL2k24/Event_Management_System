using EventManagementSystem.Application.Commands.CancelEvent;
using EventManagementSystem.Application.Commands.CreateEvent;
using EventManagementSystem.Application.Commands.CreateTicketCategory;
using EventManagementSystem.Application.Commands.PublishEvent;
using EventManagementSystem.Application.Queries.GetAvailableEvents;
using EventManagementSystem.Application.Queries.GetEventById;
using EventManagementSystem.Domain.Entities;
using EventManagementSystem.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

/*
PSEUDOCODE / PLAN (detailed):
- Problem: current code calls `new GetAvailableEventsQuery(id)` but `GetAvailableEventsQuery` expects a DateTime? parameter.
- Goal: Fix GetById to call the correct query that accepts a Guid event id.
- Steps:
  1. Use (or create) `GetEventByIdQuery` that accepts a Guid in its constructor (based on project query files).
  2. Replace the mediator call in `GetById` to send `new GetEventByIdQuery(id)`.
  3. Preserve behavior: if result is null -> return NotFound(), else return Ok(result).
  4. Keep existing route and attributes unchanged.
- Edge cases:
  - If `GetEventByIdQuery` namespace/name differs in project, adjust using/import accordingly.
  - Ensure no conversion of Guid to DateTime is attempted.
*/

[ApiController]
[Route("api/[controller]")] // Ini akan jadi api/events
public class EventsController : ControllerBase
{
    private readonly IMediator _mediator;
    //dbcontext variable _context
    private readonly AppDbContext _context;

    public EventsController(IMediator mediator, AppDbContext context)
    {
        _mediator = mediator;
        _context = context; // DAN INI
    }

    [HttpPost] // INI WAJIB ADA agar POST /api/events bisa bekerja
    public async Task<IActionResult> Create([FromBody] CreateEventCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    //publish 
    [HttpPost("{id}/publish")] // Harus ada {id} agar parameter ID terbaca
    public async Task<IActionResult> Publish(Guid id)
    {
        var command = new PublishEventCommand(id);
        await _mediator.Send(command);
        return Ok();
    }

    [HttpPost("{id}/categories")]
    public async Task<IActionResult> AddCategory(Guid id, [FromBody] CreateTicketCategoryCommand command)
    {
        // Pastikan ID dari URL (id) sama dengan EventId di command
        if (id != command.EventId)
        {
            return BadRequest("Event ID mismatch.");
        }

        var categoryId = await _mediator.Send(command);
        return Ok(categoryId);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetEventByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound($"Event with ID {id} not found.");
        }

        return Ok(result);
    }


    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var command = new CancelEventCommand(id);
        await _mediator.Send(command);

        // 204 No Content adalah standar untuk operasi update/cancel yang sukses
        return NoContent();
    }
    [HttpPost("{id}/complete")]
    public async Task<IActionResult> Complete(Guid id)
    {
        // Sekarang _context sudah tidak null!
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

        // Menggunakan Reflection atau logic publik jika ada. 
        // Tapi karena Status private set, kita buatkan method di Event.cs saja:
        @event.ResetToPublishedForTest();
        await _context.SaveChangesAsync();
        return Ok("Status reset to Published (1)");
    }

}