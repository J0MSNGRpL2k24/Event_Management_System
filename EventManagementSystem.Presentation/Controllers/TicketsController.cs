using EventManagementSystem.Application.Commands.CheckInTicket;
using EventManagementSystem.Application.Commands.CreateTicketCategory;
using EventManagementSystem.Application.Commands.DisableTicketCategory;
using EventManagementSystem.Application.Queries.ViewPurchasedTickets;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventManagementSystem.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TicketsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // --- USER STORY 4: Create Ticket Category ---
    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateTicketCategoryCommand command)
    {
        try
        {
            var categoryId = await _mediator.Send(command);
            return Ok(new { CategoryId = categoryId, Message = "Ticket category created successfully." });
        }
        catch (Exception ex)
        {
            // Menangkap error dari Domain (misal: harga < 0, kuota melebihi kapasitas event)
            return BadRequest(new { Message = ex.Message });
        }
    }

    // --- USER STORY 5: Disable Ticket Category ---
    // Menggunakan route parameter agar URL lebih deskriptif dan mudah di-test
    [HttpPost("events/{eventId}/categories/{categoryId}/disable")]
    public async Task<IActionResult> DisableCategory(Guid eventId, Guid categoryId)
    {
        try
        {
            var command = new DisableTicketCategoryCommand(eventId, categoryId);
            await _mediator.Send(command);

            // 204 No Content adalah standar RESTful untuk update state yang berhasil
            return NoContent();
        }
        catch (Exception ex)
        {
            // Menangkap error dari Domain (misal: event sudah completed)
            return BadRequest(new { Message = ex.Message });
        }
    }

    // --- USER STORY 12: View Purchased Tickets ---
    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetCustomerTickets(Guid customerId)
    {
        try
        {
            var query = new ViewPurchasedTicketsQuery(customerId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    // --- USER STORY 13 & 14: Check In Ticket & Reject Invalid ---
    [HttpPost("checkin")]
    public async Task<IActionResult> CheckIn([FromBody] CheckInTicketCommand command)
    {
        try
        {
            await _mediator.Send(command);
            return Ok(new { Message = "Check-in successful. Participant may enter." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}