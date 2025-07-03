public class InMemoryVenueService : IVenueService
{
    private readonly ILogger<InMemoryVenueService> _logger;
    private readonly List<Venue> _venues = [];

    public InMemoryVenueService(ILogger<InMemoryVenueService> logger)
    {
        _logger = logger;
    }

    public Task<ServiceResult<IEnumerable<Venue>>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all venues");

        try
        {
            return Task.FromResult(ServiceResult<IEnumerable<Venue>>.Ok(_venues));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving venues");
            return Task.FromResult(ServiceResult<IEnumerable<Venue>>.Fail(["Error retrieving venues"]));
        }   
    }

    public Task<ServiceResult<Venue?>> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Retrieving venue with ID {VenueId}", id);

        try
        {
            var venue = _venues.FirstOrDefault(v => v.Id == id);
            if (venue == null)
            {
                _logger.LogWarning("Venue with ID {VenueId} not found", id);
                return Task.FromResult(ServiceResult<Venue?>.Fail(["Venue not found"]));
            }
            return Task.FromResult(ServiceResult<Venue?>.Ok(venue));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving venue with ID {VenueId}", id);
            return Task.FromResult(ServiceResult<Venue?>.Fail(["Error retrieving venue"]));
        }
    }

    public Task<ServiceResult<Venue>> CreateAsync(CreateVenueRequest request)
    {
        _logger.LogInformation("Creating new venue with name {VenueName}", request.Name);

        try
        {
            var venue = new Venue
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Capacity = request.Capacity,
                Address = request.Address
            };

            _venues.Add(venue);
            return Task.FromResult(ServiceResult<Venue>.Ok(venue));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating venue with name {VenueName}", request.Name);
            return Task.FromResult(ServiceResult<Venue>.Fail(["Error creating venue"]));
        }   
    }

    public Task<ServiceResult<bool>> UpdateAsync(Guid id, UpdateVenueRequest request)
    {
        _logger.LogInformation("Updating venue with ID {VenueId}", id);
        try
        {
            var venue = _venues.FirstOrDefault(v => v.Id == id);
            if (venue == null)
            {
                _logger.LogWarning("Venue with ID {VenueId} not found", id);
                return Task.FromResult(ServiceResult<bool>.Fail(["Venue not found"]));
            }

            venue.Name = !string.IsNullOrWhiteSpace(request.Name) ? request.Name : venue.Name;
            venue.Capacity = request.Capacity > 0 ? request.Capacity : venue.Capacity;
            venue.Address = !string.IsNullOrWhiteSpace(request.Address) ? request.Address : venue.Address;

            return Task.FromResult(ServiceResult<bool>.Ok(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating venue with ID {VenueId}", id);
            return Task.FromResult(ServiceResult<bool>.Fail(["Error updating venue"]));
        }
    }

    public Task<ServiceResult<bool>> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting venue with ID {VenueId}", id);
        try
        {
            var venue = _venues.FirstOrDefault(v => v.Id == id);
            if (venue == null)
            {
                _logger.LogWarning("Venue with ID {VenueId} not found", id);
                return Task.FromResult(ServiceResult<bool>.Fail(["Venue not found"]));
            }

            _venues.Remove(venue);
            return Task.FromResult(ServiceResult<bool>.Ok(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting venue with ID {VenueId}", id);
            return Task.FromResult(ServiceResult<bool>.Fail(["Error deleting venue"]));
        }
    }
}