using EventManagementSystem.Application.Commands.ConfirmPayment;
using EventManagementSystem.Application.Commands.CreateBooking;
using EventManagementSystem.Application.Commands.ExpireBooking;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventManagementSystem.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BookingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingCommand command)
    {
        try
        {
            var bookingId = await _mediator.Send(command);
            return Ok(new { Id = bookingId, Message = "Tickets reserved! Please pay within 15 minutes." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    // --- TAMBAHAN BARU UNTUK SECTION PAYMENT ---

    [HttpPost("{id}/pay")]
    public async Task<IActionResult> ConfirmPayment(Guid id, [FromBody] ConfirmPaymentRequest request)
    {
        try
        {
            // Menggabungkan ID dari URL dan Amount dari Body JSON
            var command = new ConfirmPaymentCommand(id, request.AmountPaid);
            await _mediator.Send(command);

            return Ok(new { Message = "Payment successful. Tickets have been issued." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("{id}/expire")]
    public async Task<IActionResult> ExpireBooking(Guid id)
    {
        try
        {
            // Eksekusi ini bisa dipanggil manual untuk mendemonstrasikan sistem kedaluwarsa
            var command = new ExpireBookingCommand(id);
            await _mediator.Send(command);

            return Ok(new { Message = "Booking expired. Reserved ticket quota has been released." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}

// Class bantuan (*DTO*) agar JSON dari Postman bisa ditangkap dengan rapi
public class ConfirmPaymentRequest
{
    public decimal AmountPaid { get; set; }
}