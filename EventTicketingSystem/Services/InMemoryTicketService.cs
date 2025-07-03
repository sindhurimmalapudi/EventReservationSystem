public class InMemoryTicketService : ITicketService, IDisposable
{
    private readonly IEventService _eventService;
    private readonly TimeSpan _reservationWindow = TimeSpan.FromMinutes(15);
    private readonly ILogger<InMemoryTicketService> _logger;
    private readonly IPaymentService _paymentService;
    private readonly ICache<Guid, Ticket> _ticketCache;
    private readonly IDictionary<Guid, Ticket> _confirmedTickets = new Dictionary<Guid, Ticket>();
    private Timer? _timer;

    public InMemoryTicketService(IEventService eventService, ICache<Guid, Ticket> ticketCache, IPaymentService paymentService, ILogger<InMemoryTicketService> logger)
    {
        _eventService = eventService;
        _logger = logger;
        _ticketCache = ticketCache;
        _paymentService = paymentService;
        _timer = new Timer(ClearExpiredItemsFromCache, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
    }

    public async Task<ServiceResult<Ticket?>> ReserveTicketAsync(Guid eventId, Guid ticketTypeId, int seatQuantity = 1)
    {
        _logger.LogInformation($"Reserving ticket for event {eventId}, ticket type {ticketTypeId}, quantity {seatQuantity}");
        try
        {
            if (seatQuantity <= 0)
                return ServiceResult<Ticket?>.Fail(["Seat quantity must be greater than zero"]);

            var ev = await _eventService.GetEventByIdAsync(eventId);
            if (ev == null) return ServiceResult<Ticket?>.Fail(["Event not found"]);

            TicketType? ticketType = ev.Data?.TicketTypes.FirstOrDefault(tt => tt.Id == ticketTypeId);
            if (ticketType == null) return ServiceResult<Ticket?>.Fail(["Ticket type not found"]);

            lock (ticketType.LockObj)
            {

                if (ticketType.AvailableCapacity < seatQuantity)
                    return ServiceResult<Ticket?>.Fail(["Insufficient available capacity"]);

                var ticket = new Ticket
                {
                    Id = Guid.NewGuid(),
                    TicketTypeId = ticketTypeId,
                    TicketType = ticketType,
                    Status = TicketStatus.Reserved,
                    SeatQuantity = seatQuantity,
                    ReservedAt = DateTime.UtcNow,
                    ExpiryTime = DateTime.UtcNow.AddMinutes(_reservationWindow.TotalMinutes)
                };

                _ticketCache.Set(ticket.Id, ticket, _reservationWindow);
                ticketType.AvailableCapacity -= seatQuantity;

                return ServiceResult<Ticket?>.Ok(ticket);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error reserving ticket for event {eventId} and ticket type {ticketTypeId} with quantity {seatQuantity}");
            return ServiceResult<Ticket?>.Fail(["Error reserving ticket"]);
        }
    }

    public async Task<ServiceResult<Ticket?>> ConfirmTicketAsync(Guid ticketId)
    {
        _logger.LogInformation($"Confirming ticket {ticketId}");
        try
        {
            var ticket = _ticketCache.TryGet(ticketId, out var cachedTicket) ? cachedTicket : null;
            if (ticket == null)
                return ServiceResult<Ticket?>.Fail(["Ticket not found"]);

            _ticketCache.Remove(ticketId);
            if (ticket.ExpiryTime < DateTime.UtcNow)
            {
                ticket.TicketType.AvailableCapacity += ticket.SeatQuantity;
                ticket.Status = TicketStatus.Failed; // Mark as cancelled due to expiry
                _logger.LogWarning($"Ticket {ticketId} reservation has expired and cannot be confirmed.");
                return ServiceResult<Ticket?>.Fail(["Ticket's reservation has expired, cannot book."]);
            }

            //call the payment service here
            var paymentResult = await _paymentService.ProcessPaymentAsync(ticket.TicketType.Price * ticket.SeatQuantity, new { /* payment details */ });
            if (!paymentResult.Data)
            {
                ticket.TicketType.AvailableCapacity += ticket.SeatQuantity;
                ticket.Status = TicketStatus.Failed; // Mark as cancelled due to payment failure
                _logger.LogWarning($"Payment failed for ticket {ticketId}");
                return ServiceResult<Ticket?>.Fail(["Payment failed, cannot confirm ticket."]);
            }

            var ticketType = ticket.TicketType;
            lock (ticketType.LockObj)
            {
                ticket.Status = TicketStatus.Confirmed;
                ticket.ExpiryTime = null; // No longer needed after confirmation
                ticket.ConfirmedAt = DateTime.UtcNow;
                _confirmedTickets[ticketId] = ticket; // Store confirmed ticket
                return ServiceResult<Ticket?>.Ok(ticket);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error confirming ticket {ticketId}");
            return ServiceResult<Ticket?>.Fail(["Error confirming ticket"]);
        }
    }

    public Task<ServiceResult<bool>> CancelReservationAsync(Guid ticketId)
    {
        _logger.LogInformation($"Cancelling reservation for ticket {ticketId}");
        try
        {
            var ticket = _confirmedTickets.TryGetValue(ticketId, out var confirmedTicket) ? confirmedTicket : null;
            if (ticket == null)
            {
                _logger.LogWarning($"Ticket {ticketId} not found for cancellation.");
                return Task.FromResult(ServiceResult<bool>.Fail(["Ticket not found"]));
            }

            var lockObj = ticket.TicketType.LockObj;
            lock (lockObj)
            {
                ticket.Status = TicketStatus.Cancelled;
                //process refund logic here if needed
                ticket.TicketType.AvailableCapacity += ticket.SeatQuantity;
            }
            _confirmedTickets.Remove(ticketId);

            return Task.FromResult(ServiceResult<bool>.Ok(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error cancelling reservation for ticket {ticketId}");
            return Task.FromResult(ServiceResult<bool>.Fail(["Error cancelling reservation"]));
        }
    }

    public async Task<ServiceResult<TicketAvailabilityResponse>> GetAvailableTicketsAsync(Guid eventId)
    {
        _logger.LogInformation($"Getting available tickets for event {eventId}");

        try
        {
            var ev = await _eventService.GetEventByIdAsync(eventId);
            var ticketTypes = ev?.Data?.TicketTypes;

            if (ticketTypes == null || !ticketTypes.Any())
                return ServiceResult<TicketAvailabilityResponse>.Fail(["No ticket types found"]);

            var availableTickets = ticketTypes.Select(t => new 
            {
                TicketTypeId = t.Id,
                TicketTypeName = t.Name,
                AvailableCount = t.AvailableCapacity
            }).ToList();

            return ServiceResult<TicketAvailabilityResponse>.Ok(new TicketAvailabilityResponse
            {
                EventId = eventId,
                TicketTypeAvailability = availableTickets.Select(t => (t.TicketTypeId, t.TicketTypeName, t.AvailableCount)).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting available tickets for event {eventId}");
            return ServiceResult<TicketAvailabilityResponse>.Fail(["Error getting available tickets"]);
        }
    }

    public void Dispose()
    {
         _timer?.Dispose();
    }
    
    private void ClearExpiredItemsFromCache(object? state)
    {
        _logger.LogInformation("Running scheduled task to clear expired tickets");
        try
        {
            foreach (var ticket in _ticketCache.GetExpiredValues())
            {
                _ticketCache.Remove(ticket.Id);
                ticket.TicketType.AvailableCapacity += ticket.SeatQuantity;
                ticket.Status = TicketStatus.Failed; // Mark as cancelled due to expiry
                _logger.LogInformation($"Ticket {ticket.Id} has expired and was removed from cache.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during scheduled task execution");
        }
    }
}
