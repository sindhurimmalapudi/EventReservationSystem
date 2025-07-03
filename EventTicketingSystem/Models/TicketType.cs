public class TicketType
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public int AvailableCapacity { get; set; }
    public int TotalCapacity { get; set; }
    public Guid EventId { get; set; }
    public Event? Event { get; set; }
    public object LockObj { get; } = new();
}