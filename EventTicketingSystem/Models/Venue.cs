public class Venue
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Address { get; set; }
    public int Capacity { get; set; }
    public List<(Guid EventId, DateTime Date)> Events { get; set; } = [];
}