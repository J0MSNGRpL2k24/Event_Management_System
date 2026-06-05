using EventManagementSystem.Application.Commands.ApproveRefund;
using EventManagementSystem.Application.Commands.MarkRefundAsPaidOut;
using EventManagementSystem.Application.Commands.RejectRefund;
using EventManagementSystem.Application.Commands.RequestRefund;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventManagementSystem.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RefundsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RefundsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // --- USER STORY 15: Request Refund ---
    [HttpPost]
    public async Task<IActionResult> RequestRefund([FromBody] RequestRefundCommand command)
    {
        try
        {
            var refundId = await _mediator.Send(command);
            return Ok(new { Id = refundId, Message = "Refund requested successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    // --- USER STORY 16: Approve Refund ---
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveRefund(Guid id)
    {
        try
        {
            await _mediator.Send(new ApproveRefundCommand(id));
            return Ok(new { Message = "Refund approved. Tickets have been cancelled." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    // --- USER STORY 17: Reject Refund ---
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectRefund(Guid id, [FromBody] RejectRefundDto request)
    {
        try
        {
            var command = new RejectRefundCommand(id, request.RejectionReason);
            await _mediator.Send(command);
            return Ok(new { Message = "Refund rejected." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    // --- USER STORY 18: Mark Refund as Paid Out ---
    [HttpPost("{id}/payout")]
    public async Task<IActionResult> MarkAsPaidOut(Guid id, [FromBody] PayoutRefundDto request)
    {
        try
        {
            var command = new MarkRefundAsPaidOutCommand(id, request.PaymentReference);
            await _mediator.Send(command);
            return Ok(new { Message = "Refund has been paid out successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}

// Class bantuan (*DTO*) untuk menangkap data dari Body JSON Postman
public class RejectRefundDto
{
    public string RejectionReason { get; set; } = string.Empty;
}

public class PayoutRefundDto
{
    public string PaymentReference { get; set; } = string.Empty;
}