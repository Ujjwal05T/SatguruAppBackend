using Microsoft.EntityFrameworkCore;
using WastageUploadService.Data;
using WastageUploadService.Models;

namespace WastageUploadService.Services;

public interface IWastageService
{
    Task<WastageResponseDto> CreateOrUpdateWastageAsync(CreateWastageDto dto);
    Task<WastageResponseDto?> GetWastageByIdAsync(int id);
    Task<WastageResponseDto?> GetWastageByChallanIdAsync(string inwardChallanId);
    Task<List<WastageResponseDto>> GetAllWastagesAsync();
    Task<bool> DeleteWastageAsync(int id);
}

public class WastageService : IWastageService
{
    private readonly AppDbContext _context;
    private readonly IFileService _fileService;
    private readonly ILogger<WastageService> _logger;
    private readonly IInwardChallanService _inwardChallanService;

    public WastageService(AppDbContext context, IFileService fileService, ILogger<WastageService> logger, IInwardChallanService inwardChallanService)
    {
        _context = context;
        _fileService = fileService;
        _logger = logger;
        _inwardChallanService = inwardChallanService;
    }

    public async Task<WastageResponseDto> CreateOrUpdateWastageAsync(CreateWastageDto dto)
    {
        try
        {
            // Check if wastage already exists for this challan
            var existingWastage = await _context.Wastages
                .FirstOrDefaultAsync(w => w.InwardChallanId == dto.InwardChallanId);

            bool isUpdate = existingWastage != null;

            Wastage wastage;

            if (isUpdate)
            {
                // Update existing wastage
                wastage = existingWastage!;

                // Save new images if provided
                if (dto.ImageFiles != null && dto.ImageFiles.Any())
                {
                    var newImageUrls = await _fileService.SaveImagesAsync(dto.ImageFiles, dto.InwardChallanId);

                    // Merge with existing images
                    var existingUrls = wastage.ImageUrls;
                    existingUrls.AddRange(newImageUrls);
                    wastage.ImageUrls = existingUrls;
                }

                // Update properties
                wastage.PartyName = dto.PartyName;
                wastage.VehicleNo = dto.VehicleNo;
                wastage.Date = dto.Date;
                wastage.MouReport = dto.MouReport ?? new List<decimal>();
                wastage.UpdatedAt = DateTime.UtcNow;

                _context.Wastages.Update(wastage);
            }
            else
            {
                // Create new wastage
                wastage = new Wastage
                {
                    InwardChallanId = dto.InwardChallanId,
                    PartyName = dto.PartyName,
                    VehicleNo = dto.VehicleNo,
                    Date = dto.Date,
                    MouReport = dto.MouReport ?? new List<decimal>(),
                    CreatedAt = DateTime.UtcNow
                };

                // Save images
                if (dto.ImageFiles != null && dto.ImageFiles.Any())
                {
                    var savedImageUrls = await _fileService.SaveImagesAsync(dto.ImageFiles, dto.InwardChallanId);
                    wastage.ImageUrls = savedImageUrls;
                }

                await _context.Wastages.AddAsync(wastage);
            }

            await _context.SaveChangesAsync();

            // Calculate MOU average and update inward challan if MOU reports exist
            decimal? mouAverage = null;
            if (wastage.MouReport != null && wastage.MouReport.Any())
            {
                mouAverage = wastage.MouReport.Average();
                _logger.LogInformation($"Calculated MOU average for challan {dto.InwardChallanId}: {mouAverage}");

                // Call Python backend to update inward challan
                try
                {
                    var updateSuccess = await _inwardChallanService.UpdateMouReportAsync(dto.InwardChallanId, mouAverage.Value);
                    if (updateSuccess)
                    {
                        _logger.LogInformation($"Successfully updated inward challan MOU report for {dto.InwardChallanId}");
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to update inward challan MOU report for {dto.InwardChallanId}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error calling inward challan service for {dto.InwardChallanId}");
                    // Don't fail the wastage operation if inward challan update fails
                }
            }

            _logger.LogInformation($"Wastage {(isUpdate ? "updated" : "created")} for challan: {dto.InwardChallanId}" +
                (mouAverage.HasValue ? $" with MOU average: {mouAverage}" : ""));

            return new WastageResponseDto
            {
                Id = wastage.Id,
                InwardChallanId = wastage.InwardChallanId,
                PartyName = wastage.PartyName,
                VehicleNo = wastage.VehicleNo,
                Date = wastage.Date,
                MouReport = wastage.MouReport,
                ImageUrls = wastage.ImageUrls,
                CreatedAt = wastage.CreatedAt,
                UpdatedAt = wastage.UpdatedAt,
                IsUpdate = isUpdate,
                MouAverage = mouAverage // Add MOU average to response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating/updating wastage for challan: {dto.InwardChallanId}");
            throw;
        }
    }

    public async Task<WastageResponseDto?> GetWastageByIdAsync(int id)
    {
        var wastage = await _context.Wastages
            .FirstOrDefaultAsync(w => w.Id == id);

        if (wastage == null)
            return null;

        return new WastageResponseDto
        {
            Id = wastage.Id,
            InwardChallanId = wastage.InwardChallanId,
            PartyName = wastage.PartyName,
            VehicleNo = wastage.VehicleNo,
            Date = wastage.Date,
            MouReport = wastage.MouReport,
            ImageUrls = wastage.ImageUrls,
            CreatedAt = wastage.CreatedAt,
            UpdatedAt = wastage.UpdatedAt,
            IsUpdate = false
        };
    }

    public async Task<WastageResponseDto?> GetWastageByChallanIdAsync(string inwardChallanId)
    {
        var wastage = await _context.Wastages
            .FirstOrDefaultAsync(w => w.InwardChallanId == inwardChallanId);

        if (wastage == null)
            return null;

        return new WastageResponseDto
        {
            Id = wastage.Id,
            InwardChallanId = wastage.InwardChallanId,
            PartyName = wastage.PartyName,
            VehicleNo = wastage.VehicleNo,
            Date = wastage.Date,
            MouReport = wastage.MouReport,
            ImageUrls = wastage.ImageUrls,
            CreatedAt = wastage.CreatedAt,
            UpdatedAt = wastage.UpdatedAt
        };
    }

    public async Task<List<WastageResponseDto>> GetAllWastagesAsync()
    {
        var wastages = await _context.Wastages
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();

        return wastages.Select(w => new WastageResponseDto
        {
            Id = w.Id,
            InwardChallanId = w.InwardChallanId,
            PartyName = w.PartyName,
            VehicleNo = w.VehicleNo,
            Date = w.Date,
            MouReport = w.MouReport,
            ImageUrls = w.ImageUrls,
            CreatedAt = w.CreatedAt,
            UpdatedAt = w.UpdatedAt
        }).ToList();
    }

    public async Task<bool> DeleteWastageAsync(int id)
    {
        var wastage = await _context.Wastages.FindAsync(id);

        if (wastage == null)
            return false;

        // Delete associated images
        await _fileService.DeleteImagesAsync(wastage.ImageUrls);

        _context.Wastages.Remove(wastage);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Wastage deleted: {id}");

        return true;
    }
}
