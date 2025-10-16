using System.Net.Http.Headers;
using System.Text.Json;
using System.Linq;

namespace WastageUploadService.Services;

public interface IInwardChallanService
{
    Task<bool> UpdateMouReportAsync(string challanId, decimal mouAverage);
}

public class InwardChallanService : IInwardChallanService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<InwardChallanService> _logger;
    private readonly IConfiguration _config;

    public InwardChallanService(HttpClient httpClient, ILogger<InwardChallanService> logger, IConfiguration config)
    {
        _httpClient = httpClient;
        _logger = logger;
        _config = config;

        // Configure HttpClient
        var pythonApiUrl = _config["PythonApiUrl"] ?? "http://localhost:8000";
        _httpClient.BaseAddress = new Uri(pythonApiUrl);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Add API key if configured
        var apiKey = _config["InwardChallanApiKey"];
        _logger.LogInformation("Configuring API key: {ApiKey}", string.IsNullOrEmpty(apiKey) ? "NOT SET" : "SET");

        if (!string.IsNullOrEmpty(apiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
            _logger.LogInformation("API key header added to HTTP client");
        }
        else
        {
            _logger.LogWarning("API key not found in configuration. Expected key: 'InwardChallanApiKey'");
        }
    }

    public async Task<bool> UpdateMouReportAsync(string challanId, decimal mouAverage)
    {
        if (string.IsNullOrWhiteSpace(challanId))
        {
            _logger.LogWarning("Challan ID is null or empty");
            return false;
        }

        if (mouAverage < 0)
        {
            _logger.LogWarning("Invalid MOU average value: {MouAverage}", mouAverage);
            return false;
        }

        var maxRetries = 3;
        var baseDelay = TimeSpan.FromSeconds(1);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                _logger.LogInformation("Attempt {Attempt} to update MOU report for challan {ChallanId} with value {MouAverage}",
                    attempt, challanId, mouAverage);

                var requestData = new
                {
                    challan_id = challanId,
                    mou_average = mouAverage
                };

                var jsonContent = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                // Log request details for debugging
                _logger.LogInformation("Sending POST request to: {Url}", "/api/inward-challan/update-mou-from-wastage");
                _logger.LogInformation("Request body: {RequestBody}", jsonContent);
                _logger.LogInformation("Request headers: {Headers}", string.Join(", ", _httpClient.DefaultRequestHeaders.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}")));

                var response = await _httpClient.PostAsync("/api/inward-challan/update-mou-from-wastage", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully updated MOU report for challan {ChallanId}", challanId);
                    return true;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Inward challan not found: {ChallanId}", challanId);
                    return false; // Don't retry 404s
                }

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Bad request when updating MOU report for challan {ChallanId}: {Error}",
                        challanId, errorContent);
                    return false; // Don't retry bad requests
                }

                _logger.LogWarning("Failed to update MOU report for challan {ChallanId}. Status: {StatusCode}",
                    challanId, response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed for challan {ChallanId} on attempt {Attempt}",
                    challanId, attempt);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request timeout for challan {ChallanId} on attempt {Attempt}",
                    challanId, attempt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating MOU report for challan {ChallanId} on attempt {Attempt}",
                    challanId, attempt);
            }

            // Don't wait on the last attempt
            if (attempt < maxRetries)
            {
                var delay = baseDelay * attempt; // Exponential backoff
                _logger.LogInformation("Waiting {Delay} seconds before retry...", delay.TotalSeconds);
                await Task.Delay(delay);
            }
        }

        _logger.LogError("Failed to update MOU report for challan {ChallanId} after {MaxRetries} attempts",
            challanId, maxRetries);
        return false;
    }
}