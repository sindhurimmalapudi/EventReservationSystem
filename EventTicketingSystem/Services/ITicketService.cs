public interface ITicketService
{
    Task<ServiceResult<Ticket?>> ReserveTicketAsync(Guid eventId, Guid ticketTypeId, int seatQuantity = 1);
    Task<ServiceResult<Ticket?>> ConfirmTicketAsync(Guid ticketId);
    Task<ServiceResult<bool>> CancelReservationAsync(Guid ticketId);
    Task<ServiceResult<TicketAvailabilityResponse>> GetAvailableTicketsAsync(Guid eventId);
}