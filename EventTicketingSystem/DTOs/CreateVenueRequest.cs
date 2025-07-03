using System.ComponentModel.DataAnnotations;

public class CreateVenueRequest
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Capacity { get; set; }

    [Required]
    [StringLength(500)]
    public string Address { get; set; } = string.Empty;
}