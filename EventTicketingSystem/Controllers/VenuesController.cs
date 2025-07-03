using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/venues")]
public class VenuesController : ControllerBase
{
    private readonly IVenueService _venueService;

    public VenuesController(IVenueService venueService)
    {
        _venueService = venueService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var serviceResult = await _venueService.GetAllAsync();
        if (!serviceResult.Success)
            return BadRequest(string.Join(", ", serviceResult.Errors));

        return Ok(serviceResult.Data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var serviceResult = await _venueService.GetByIdAsync(id);
        if (!serviceResult.Success)
            return BadRequest(string.Join(", ", serviceResult.Errors));

        return Ok(serviceResult.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVenueRequest request)
    {
        var serviceResult = await _venueService.CreateAsync(request);
        if (!serviceResult.Success)
            return BadRequest(string.Join(", ", serviceResult.Errors));

        return CreatedAtAction(nameof(GetById), new { id = serviceResult.Data?.Id }, serviceResult.Data);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVenueRequest request)
    {
        var serviceResult = await _venueService.UpdateAsync(id, request);
        if (!serviceResult.Success)
            return BadRequest(string.Join(", ", serviceResult.Errors));

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var serviceResult = await _venueService.DeleteAsync(id);
        if (!serviceResult.Success)
            return BadRequest(string.Join(", ", serviceResult.Errors));

        return Ok();
    }
}
