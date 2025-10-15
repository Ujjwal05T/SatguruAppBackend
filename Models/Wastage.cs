using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WastageUploadService.Models;

public class Wastage
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string InwardChallanId { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string PartyName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string VehicleNo { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }

    // Store MOU report as JSON array
    [Required]
    public string MouReportJson { get; set; } = "[]";

    // Store image URLs as JSON array
    [Required]
    public string ImageUrlsJson { get; set; } = "[]";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties for JSON deserialization
    [NotMapped]
    public List<decimal> MouReport
    {
        get => string.IsNullOrEmpty(MouReportJson)
            ? new List<decimal>()
            : System.Text.Json.JsonSerializer.Deserialize<List<decimal>>(MouReportJson) ?? new List<decimal>();
        set => MouReportJson = System.Text.Json.JsonSerializer.Serialize(value);
    }

    [NotMapped]
    public List<string> ImageUrls
    {
        get => string.IsNullOrEmpty(ImageUrlsJson)
            ? new List<string>()
            : System.Text.Json.JsonSerializer.Deserialize<List<string>>(ImageUrlsJson) ?? new List<string>();
        set => ImageUrlsJson = System.Text.Json.JsonSerializer.Serialize(value);
    }
}
