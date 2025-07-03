public class InMemoryEventService : IEventService
{
    private readonly ILogger<InMemoryEventService> _logger;
    private readonly IVenueService _venueService;

    private readonly List<Event> _events = new();

    public InMemoryEventService(IVenueService venueService, ILogger<InMemoryEventService> logger)
    {
        _venueService = venueService;
        _logger = logger;
    }

    public Task<ServiceResult<IEnumerable<Event>>> GetAllEventsAsync()
    {
        _logger.LogInformation("Retrieving all events");
        try
        {
            return Task.FromResult(ServiceResult<IEnumerable<Event>>.Ok(_events));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving events");
            return Task.FromResult(ServiceResult<IEnumerable<Event>>.Fail(["Error retrieving events"]));
        }
    }   

    public async Task<ServiceResult<Event>> CreateEventAsync(CreateEventRequest request)
    {
        _logger.LogInformation("Creating new event with name {EventName}", request.Name);

        try
        {
            var venue = await _venueService.GetByIdAsync(request.VenueId);
            if (venue == null)
            {
                return ServiceResult<Event>.Fail(["Venue not found"]);
            }

            List<string> errors = [];
            int requestedCapacity = request.TicketTypes.Sum(tt => tt.Capacity);
            if (requestedCapacity > venue.Data?.Capacity)
            {
                errors.Add("Requested capacity exceeds venue capacity");
            }

            if (venue.Data?.Events.Any(e => e.Date == request.Date) == true)
            {
                errors.Add("An event with the same name and date already exists at this venue");
            }

            if (errors.Any())
            {
                return ServiceResult<Event>.Fail(errors);
            }

            var newEventId = Guid.NewGuid();
            var newEvent = new Event
            {
                Id = newEventId,
                Name = request.Name,
                Description = request.Description,
                Date = request.Date,
                VenueId = request.VenueId,
                Venue = venue.Data ?? throw new InvalidOperationException("Venue data is null"),
                TicketTypes = [.. request.TicketTypes.Select(tt => new TicketType
                {
                    Id = Guid.NewGuid(),
                    Name = tt.Name,
                    Price = tt.Price,
                    AvailableCapacity = tt.Capacity,
                    TotalCapacity = tt.Capacity,
                    EventId = newEventId
                })]
            };
            newEvent.Venue.Events.Add((newEvent.Id, newEvent.Date));
            _events.Add(newEvent);
            return ServiceResult<Event>.Ok(newEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Event creation failed");
            return ServiceResult<Event>.Fail(["Invalid request data"]);
        }
    }

    public Task<ServiceResult<Event?>> GetEventByIdAsync(Guid eventId)
    {
        _logger.LogInformation("Retrieving event with ID {EventId}", eventId);
        try
        {
            var eventObj = _events.FirstOrDefault(e => e.Id == eventId);
            if (eventObj == null)
            {
                return Task.FromResult(ServiceResult<Event?>.Fail(["Event not found"]));
            }
            return Task.FromResult(ServiceResult<Event?>.Ok(eventObj));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event with ID {EventId}", eventId);
            return Task.FromResult(ServiceResult<Event?>.Fail(["Error retrieving event"]));
        }
    }

    public  Task<ServiceResult<bool>> UpdateEventAsync(Guid eventId, UpdateEventRequest request)
    {
        try
        {
            _logger.LogInformation("Updating event with ID {EventId}", eventId);
            
            var eventToUpdate = _events.FirstOrDefault(e => e.Id == eventId);
            if (eventToUpdate == null)
            {
                return Task.FromResult(ServiceResult<bool>.Fail(["Event not found"]));
            }
            
            if (eventToUpdate.Published == true)
            {
                return Task.FromResult(ServiceResult<bool>.Fail(["Cannot update a published event"]));
            }

            var venue = eventToUpdate.Venue;
            if (venue != null && request.Date != null)
            {
                // Check if the new date conflicts with existing events at the venue
                if (venue.Events.Any(e => e.Date == request.Date && e.EventId != eventId))
                {
                    return Task.FromResult(ServiceResult<bool>.Fail(["An event with the same date already exists at this venue"]));
                }
            }

            eventToUpdate.Name = request.Name ?? eventToUpdate.Name;
            eventToUpdate.Description = request.Description ?? eventToUpdate.Description;
            eventToUpdate.Date = request.Date ?? eventToUpdate.Date;

            if (request.TicketTypes != null && request.TicketTypes.Any())
            {
                // Validate total capacity against venue capacity
                int totalCapacity = request.TicketTypes.Sum(tt => tt.Capacity);
                if (totalCapacity > venue?.Capacity)
                {
                    return Task.FromResult(ServiceResult<bool>.Fail(["Total ticket capacity exceeds venue capacity"]));
                }

                // Clear existing ticket types and add new ones
                eventToUpdate.TicketTypes.Clear();
                foreach (var tt in request.TicketTypes)
                {
                    eventToUpdate.TicketTypes.Add(new TicketType
                    {
                        Id = Guid.NewGuid(),
                        Name = tt.Name,
                        Price = tt.Price,
                        TotalCapacity = tt.Capacity,
                        AvailableCapacity = tt.Capacity, // Initially all tickets are available
                        EventId = eventId
                    });
                }
            }   

            return Task.FromResult(ServiceResult<bool>.Ok(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating event with ID {EventId}", eventId);
            return Task.FromResult(ServiceResult<bool>.Fail(["Error updating event"]));
        }
    }
    
    public Task<ServiceResult<bool>> PublishEventAsync(Guid eventId)
    {
        try
        {
            _logger.LogInformation("Publishing event with ID {EventId}", eventId);
            var eventObj = _events.FirstOrDefault(e => e.Id == eventId);
            if (eventObj == null)
            {
                return Task.FromResult(ServiceResult<bool>.Fail(["Event not found"]));
            }

            if (eventObj.Published == true)
            {
                return Task.FromResult(ServiceResult<bool>.Fail(["Event is already published"]));
            }

            eventObj.Published = true;
            return Task.FromResult(ServiceResult<bool>.Ok(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event with ID {EventId}", eventId);
            return Task.FromResult(ServiceResult<bool>.Fail(["Error publishing event"]));
        }
    }

    public Task<ServiceResult<bool>> SetTicketTypesAsync(Guid eventId, List<TicketTypeRequest> ticketTypes)
    {
        try
        {
            _logger.LogInformation("Setting ticket types for event with ID {EventId}", eventId);
            var eventObj = _events.FirstOrDefault(e => e.Id == eventId);
            if (eventObj == null)
            {
                return Task.FromResult(ServiceResult<bool>.Fail(["Event not found"]));
            }

            if (eventObj.Published == true)
            {
                return Task.FromResult(ServiceResult<bool>.Fail(["Cannot set ticket types for a published event"]));
            }

            int capacitySum = ticketTypes.Sum(tt => tt.Capacity);
            if (capacitySum > eventObj.Venue.Capacity)
            {
                return Task.FromResult(ServiceResult<bool>.Fail(["Total ticket capacity exceeds venue capacity"]));
            }

            eventObj.TicketTypes.Clear();
            foreach (var tt in ticketTypes)
            {
                eventObj.TicketTypes.Add(new TicketType
                {
                    Id = Guid.NewGuid(),
                    Name = tt.Name,
                    Price = tt.Price,
                    TotalCapacity = tt.Capacity,
                    AvailableCapacity = tt.Capacity, // Initially all tickets are available
                    EventId = eventId
                });
            }

            return Task.FromResult(ServiceResult<bool>.Ok(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting ticket types for event with ID {EventId}", eventId);
            return Task.FromResult(ServiceResult<bool>.Fail(["Error setting ticket types"]));
        }
    }
}