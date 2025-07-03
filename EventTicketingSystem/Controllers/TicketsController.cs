using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/tickets")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpPost("reserve")]
    public async Task<IActionResult> Reserve([FromBody] ReserveTicketRequest request)
    {
        var serviceResult = await _ticketService.ReserveTicketAsync(request.EventId, request.TicketTypeId, request.Quantity);
        if (!serviceResult.Success)
            return BadRequest(string.Join(", ", serviceResult.Errors));

        return Ok(serviceResult.Data);
    }

    [HttpPost("purchase")]
    public async Task<IActionResult> Purchase([FromBody] PurchaseTicketRequest request)
    {
        var serviceResult = await _ticketService.ConfirmTicketAsync(request.TicketId);
        if (!serviceResult.Success)
            return BadRequest(string.Join(", ", serviceResult.Errors));

        return Ok(serviceResult.Data);
    }

    [HttpPost("cancel/{ticketId}")]
    public async Task<IActionResult> Cancel(Guid ticketId)
    {
        var serviceResult = await _ticketService.CancelReservationAsync(ticketId);
        if (!serviceResult.Success)
            return BadRequest(string.Join(", ", serviceResult.Errors));

        return Ok();
    }

    [HttpGet("availability/{eventId}")]
    public async Task<IActionResult> GetAvailability(Guid eventId)
    {
        var serviceResult = await _ticketService.GetAvailableTicketsAsync(eventId);
        if (!serviceResult.Success)
            return BadRequest(string.Join(", ", serviceResult.Errors));

        return Ok(serviceResult.Data);
    }
}
