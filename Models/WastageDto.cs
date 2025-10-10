using System.ComponentModel.DataAnnotations;

namespace WastageUploadService.Models;

public class CreateWastageDto
{
    [Required]
    [StringLength(100)]
    public string InwardChallanId { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string PartyName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string VehicleNo { get; set; } = string.Empty;

    [StringLength(50)]
    public string? SlipNo { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public decimal NetWeight { get; set; }

    public List<decimal>? MouReport { get; set; } = new List<decimal>();

    // Images will be uploaded as IFormFile
    public List<IFormFile>? ImageFiles { get; set; }
}

public class WastageResponseDto
{
    public int Id { get; set; }
    public string InwardChallanId { get; set; } = string.Empty;
    public string PartyName { get; set; } = string.Empty;
    public string VehicleNo { get; set; } = string.Empty;
    public string? SlipNo { get; set; }
    public DateTime Date { get; set; }
    public decimal NetWeight { get; set; }
    public List<decimal> MouReport { get; set; } = new List<decimal>();
    public List<string> ImageUrls { get; set; } = new List<string>();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsUpdate { get; set; } = false;
}
