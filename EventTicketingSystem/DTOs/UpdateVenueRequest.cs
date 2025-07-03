using System.ComponentModel.DataAnnotations;

public class UpdateVenueRequest
{
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public int Capacity { get; set; }

    [StringLength(500)]
    public string Address { get; set; } = string.Empty;
}