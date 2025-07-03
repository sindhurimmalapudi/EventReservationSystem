public class Event
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public Guid VenueId { get; set; }
    public required Venue Venue { get; set; }
    public bool? Published { get; set; } = false;
    public List<TicketType> TicketTypes { get; set; } = [];
}
