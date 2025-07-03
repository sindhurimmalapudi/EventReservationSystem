using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var serviceResult = await _eventService.GetAllEventsAsync();
        if (!serviceResult.Success)
            return BadRequest(string.Join(", ", serviceResult.Errors)); 

        return Ok(serviceResult.Data);
    }   

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var serviceResult = await _eventService.GetEventByIdAsync(id);
        if (!serviceResult.Success)
            return BadRequest(string.Join(", ", serviceResult.Errors));

        return Ok(serviceResult.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEventRequest request)
    {
        var serviceResult = await _eventService.CreateEventAsync(request);
        if (!serviceResult.Success)
            return BadRequest(string.Join(", ", serviceResult.Errors));

        return CreatedAtAction(nameof(GetById), new { id = serviceResult.Data?.Id }, serviceResult.Data);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEventRequest request)
    {
        var serviceResult = await _eventService.UpdateEventAsync(id, request);
        if (!serviceResult.Success)
            return BadRequest(string.Join(", ", serviceResult.Errors));

        return NoContent();
    }

    [HttpPut("{id}/publish")]
    public async Task<IActionResult> Publish(Guid id)
    {
        var serviceResult = await _eventService.PublishEventAsync(id);
        if (!serviceResult.Success)
            return BadRequest(string.Join(", ", serviceResult.Errors));

        return NoContent();
    }

    [HttpPut("{id}/tickets")]
    public async Task<IActionResult> SetTicketTypes(Guid id, [FromBody] List<TicketTypeRequest> ticketTypes)
    {
        var serviceResult = await _eventService.SetTicketTypesAsync(id, ticketTypes);
        if (!serviceResult.Success)
            return BadRequest(string.Join(", ", serviceResult.Errors));

        return NoContent();
    }
}
