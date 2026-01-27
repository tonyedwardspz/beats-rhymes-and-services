using App.Models;
using SharedLibrary.Models;
using TranscriptionMetrics = App.Models.TranscriptionMetrics;

namespace App.Services;

/// <summary>
/// Service for fetching transcription metrics from the WhisperAPI
/// </summary>
public class MetricsApiService : BaseApiService, IMetricsApiService
{
    public MetricsApiService(
        HttpClient httpClient, 
        ILogger<MetricsApiService> logger,
        IOptions<ApiConfiguration> config)
        : base(httpClient, logger)
    {
        var configValue = config.Value;
        HttpClient.BaseAddress = new Uri(configValue.WhisperBaseURL);
        HttpClient.Timeout = configValue.Timeout;
    }

    /// <inheritdoc />
    public async Task<ApiResult<List<TranscriptionMetrics>>> GetMetricsAsync()
    {
        try
        {
            Logger.LogInformation("GetMetrics: requesting from /api/whisper/metrics");
            
            var response = await HttpClient.GetAsync("/api/whisper/metrics");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var metricsContainer = JsonSerializer.Deserialize<MetricsContainer>(content, JsonOptions);
                
                if (metricsContainer?.TranscriptionMetrics != null)
                {
                    Logger.LogInformation("GetMetrics completed successfully. Fetched {Count} metrics", 
                        metricsContainer.TranscriptionMetrics.Count);
                    
                    // Convert from SharedLibrary.Models.TranscriptionMetrics to App.Models.TranscriptionMetrics
                    var mauiMetrics = metricsContainer.TranscriptionMetrics
                        .Select(MapToAppMetrics)
                        .ToList();
                    
                    return ApiResult<List<TranscriptionMetrics>>.Success(mauiMetrics);
                }
                
                return ApiResult<List<TranscriptionMetrics>>.Failure("Failed to deserialize metrics response");
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            Logger.LogError("GetMetrics failed. Status: {StatusCode}, Content: {Content}", 
                response.StatusCode, errorContent);
            
            var errorMessage = TryParseErrorMessage(errorContent) 
                ?? $"Metrics API request failed with status {response.StatusCode}";
            
            return ApiResult<List<TranscriptionMetrics>>.Failure(errorMessage, (int)response.StatusCode);
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError(ex, "Network error during GetMetrics");
            return ApiResult<List<TranscriptionMetrics>>.Failure("Network error: Unable to connect to API");
        }
        catch (TaskCanceledException ex)
        {
            Logger.LogError(ex, "Request timeout during GetMetrics");
            return ApiResult<List<TranscriptionMetrics>>.Failure("Request timeout");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error during GetMetrics");
            return ApiResult<List<TranscriptionMetrics>>.Failure($"Unexpected error: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<ApiResult<bool>> ClearMetricsAsync()
    {
        return await ExecuteDeleteAsync(
            "/api/whisper/metrics",
            "ClearMetrics");
    }

    /// <summary>
    /// Maps from SharedLibrary.Models.TranscriptionMetrics to App.Models.TranscriptionMetrics
    /// </summary>
    private static TranscriptionMetrics MapToAppMetrics(SharedLibrary.Models.TranscriptionMetrics source)
    {
        return new TranscriptionMetrics
        {
            Timestamp = source.Timestamp,
            ModelName = source.ModelName,
            TranscriptionType = source.TranscriptionType,
            SessionId = source.SessionId,
            ChunkIndex = source.ChunkIndex,
            FileSizeBytes = source.FileSizeBytes,
            AudioDurationSeconds = source.AudioDurationSeconds,
            TotalTimeMs = source.TotalTimeMs,
            PreprocessingTimeMs = source.PreprocessingTimeMs,
            TranscriptionTimeMs = source.TranscriptionTimeMs,
            Success = source.Success,
            ErrorMessage = source.ErrorMessage,
            TranscribedText = source.TranscribedText
        };
    }
}
