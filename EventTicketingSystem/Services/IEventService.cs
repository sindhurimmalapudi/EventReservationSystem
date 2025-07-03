public interface IEventService
{
    Task<ServiceResult<IEnumerable<Event>>> GetAllEventsAsync();
    Task<ServiceResult<Event>> CreateEventAsync(CreateEventRequest request);
    Task<ServiceResult<Event?>> GetEventByIdAsync(Guid eventId);
    Task<ServiceResult<bool>> UpdateEventAsync(Guid eventId, UpdateEventRequest request);
    Task<ServiceResult<bool>> PublishEventAsync(Guid eventId);
    Task<ServiceResult<bool>> SetTicketTypesAsync(Guid eventId, List<TicketTypeRequest> ticketTypes);
}