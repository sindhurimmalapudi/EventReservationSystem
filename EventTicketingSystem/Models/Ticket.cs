public class Ticket
{
    public Guid Id { get; set; }
    public Guid TicketTypeId { get; set; }
    public required TicketType TicketType { get; set; }
    public int SeatQuantity { get; set; } // Number of seats reserved
    public TicketStatus Status { get; set; } // Reserved, Confirmed, Cancelled
    public DateTime ReservedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? ExpiryTime { get; set; }
}
