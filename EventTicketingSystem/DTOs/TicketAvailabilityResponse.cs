public class TicketAvailabilityResponse
{
    public Guid EventId { get; set; }
    public List<(Guid TicketTypeId, string TicketTypeName, int AvailableCount)> TicketTypeAvailability { get; set; } = new List<(Guid, string, int)>();

}