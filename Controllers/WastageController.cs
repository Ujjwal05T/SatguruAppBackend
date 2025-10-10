using Microsoft.AspNetCore.Mvc;
using WastageUploadService.Models;
using WastageUploadService.Services;

namespace WastageUploadService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WastageController : ControllerBase
{
    private readonly IWastageService _wastageService;
    private readonly ILogger<WastageController> _logger;

    public WastageController(IWastageService wastageService, ILogger<WastageController> logger)
    {
        _wastageService = wastageService;
        _logger = logger;
    }

    /// <summary>
    /// Create or update wastage entry (multipart/form-data)
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(WastageResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WastageResponseDto>> CreateOrUpdateWastage([FromForm] CreateWastageDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate that at least one image is provided for new entries
            var existingWastage = await _wastageService.GetWastageByChallanIdAsync(dto.InwardChallanId);

            if (existingWastage == null && (dto.ImageFiles == null || !dto.ImageFiles.Any()))
            {
                return BadRequest(new { message = "At least one image is required for new wastage entries" });
            }

            var result = await _wastageService.CreateOrUpdateWastageAsync(dto);

            return Ok(new
            {
                success = true,
                message = result.IsUpdate ? "Wastage updated successfully" : "Wastage created successfully",
                data = result,
                isUpdate = result.IsUpdate
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating/updating wastage");
            return StatusCode(500, new { message = "An error occurred while processing your request", error = ex.Message });
        }
    }

    /// <summary>
    /// Get wastage by inward challan ID
    /// </summary>
    [HttpGet("by-challan/{inwardChallanId}")]
    [ProducesResponseType(typeof(WastageResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WastageResponseDto>> GetWastageByChallanId(string inwardChallanId)
    {
        try
        {
            var wastage = await _wastageService.GetWastageByChallanIdAsync(inwardChallanId);

            if (wastage == null)
            {
                return NotFound(new { message = $"No wastage found for challan ID: {inwardChallanId}" });
            }

            return Ok(wastage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting wastage for challan: {inwardChallanId}");
            return StatusCode(500, new { message = "An error occurred while retrieving wastage", error = ex.Message });
        }
    }

    /// <summary>
    /// Get all wastage entries
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<WastageResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<WastageResponseDto>>> GetAllWastages()
    {
        try
        {
            var wastages = await _wastageService.GetAllWastagesAsync();
            return Ok(wastages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all wastages");
            return StatusCode(500, new { message = "An error occurred while retrieving wastages", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete wastage by ID
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWastage(int id)
    {
        try
        {
            var result = await _wastageService.DeleteWastageAsync(id);

            if (!result)
            {
                return NotFound(new { message = $"Wastage with ID {id} not found" });
            }

            return Ok(new { message = "Wastage deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting wastage: {id}");
            return StatusCode(500, new { message = "An error occurred while deleting wastage", error = ex.Message });
        }
    }
}
