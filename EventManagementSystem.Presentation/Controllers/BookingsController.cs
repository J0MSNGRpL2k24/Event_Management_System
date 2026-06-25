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


    [HttpPost("{id}/pay")]
    public async Task<IActionResult> ConfirmPayment(Guid id, [FromBody] ConfirmPaymentRequest request)
    {
        try
        {
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

public class ConfirmPaymentRequest
{
    public decimal AmountPaid { get; set; }
}