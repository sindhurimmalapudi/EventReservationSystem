using System.ComponentModel.DataAnnotations;

public class PurchaseTicketRequest
{
    [Required]
    public Guid TicketId { get; set; }

    [Required]
    public string PaymentMethod { get; set; } = string.Empty; // e.g., "CreditCard", "PayPal"

    //other metadata like card details, billing address, etc. can be added as needed
}