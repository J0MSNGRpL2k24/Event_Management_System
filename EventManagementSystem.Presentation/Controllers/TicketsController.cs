using EventManagementSystem.Application.Commands.CheckInTicket;
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

    // --- USER STORY 12: View Purchased Tickets ---
    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetCustomerTickets(Guid customerId)
    {
        try
        {
            var query = new ViewPurchasedTicketsQuery(customerId);
            var result = await _mediator.Send(query);

            // Jika kosong, kita tetap return 200 OK tapi dengan list kosong, 
            // karena bukan error, hanya belum punya tiket.
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

            // Jika berhasil melewati handler tanpa Exception, berarti tiket valid.
            return Ok(new { Message = "Check-in successful. Participant may enter." });
        }
        catch (Exception ex)
        {
            // Menangkap dan mengembalikan pesan error persis sesuai Acceptance Criteria 14
            // (e.g., "The ticket is invalid", "Ticket already used", dll.)
            return BadRequest(new { Message = ex.Message });
        }
    }
}