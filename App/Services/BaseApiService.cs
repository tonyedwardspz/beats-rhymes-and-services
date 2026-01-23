using SharedLibrary.Models;

namespace App.Services;

/// <summary>
/// Base class for API services with common HTTP handling patterns
/// </summary>
public abstract class BaseApiService
{
    protected readonly HttpClient HttpClient;
    protected readonly ILogger Logger;
    protected readonly JsonSerializerOptions JsonOptions;

    protected BaseApiService(HttpClient httpClient, ILogger logger)
    {
        HttpClient = httpClient;
        Logger = logger;
        
        JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Executes a GET request and deserializes the response
    /// </summary>
    protected async Task<ApiResult<T>> ExecuteGetAsync<T>(
        string endpoint,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("{Operation}: requesting from {Endpoint}", operationName, endpoint);
            
            var response = await HttpClient.GetAsync(endpoint, cancellationToken);
            
            return await HandleResponse<T>(response, operationName, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            Logger.LogInformation("{Operation} was cancelled", operationName);
            return ApiResult<T>.Failure("Request was cancelled", 499);
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError(ex, "Network error during {Operation}", operationName);
            return ApiResult<T>.Failure("Network error: Unable to connect to API");
        }
        catch (TaskCanceledException ex)
        {
            Logger.LogError(ex, "Request timeout during {Operation}", operationName);
            return ApiResult<T>.Failure("Request timeout");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error during {Operation}", operationName);
            return ApiResult<T>.Failure($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes a GET request and returns the raw string response
    /// </summary>
    protected async Task<ApiResult<string>> ExecuteGetStringAsync(
        string endpoint,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("{Operation}: requesting from {Endpoint}", operationName, endpoint);
            
            var response = await HttpClient.GetAsync(endpoint, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                
                if (!string.IsNullOrEmpty(content))
                {
                    Logger.LogInformation("{Operation} completed successfully", operationName);
                    return ApiResult<string>.Success(content);
                }
                
                return ApiResult<string>.Failure("Empty response received");
            }
            
            return await HandleErrorResponse<string>(response, operationName, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            Logger.LogInformation("{Operation} was cancelled", operationName);
            return ApiResult<string>.Failure("Request was cancelled", 499);
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError(ex, "Network error during {Operation}", operationName);
            return ApiResult<string>.Failure("Network error: Unable to connect to API");
        }
        catch (TaskCanceledException ex)
        {
            Logger.LogError(ex, "Request timeout during {Operation}", operationName);
            return ApiResult<string>.Failure("Request timeout");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error during {Operation}", operationName);
            return ApiResult<string>.Failure($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes a POST request with JSON body and deserializes the response
    /// </summary>
    protected async Task<ApiResult<T>> ExecutePostAsync<T, TRequest>(
        string endpoint,
        TRequest request,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("{Operation}: posting to {Endpoint}", operationName, endpoint);
            
            var response = await HttpClient.PostAsJsonAsync(endpoint, request, JsonOptions, cancellationToken);
            
            return await HandleResponse<T>(response, operationName, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            Logger.LogInformation("{Operation} was cancelled", operationName);
            return ApiResult<T>.Failure("Request was cancelled", 499);
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError(ex, "Network error during {Operation}", operationName);
            return ApiResult<T>.Failure("Network error: Unable to connect to API");
        }
        catch (TaskCanceledException ex)
        {
            Logger.LogError(ex, "Request timeout during {Operation}", operationName);
            return ApiResult<T>.Failure("Request timeout");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error during {Operation}", operationName);
            return ApiResult<T>.Failure($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes a POST request with JSON body and returns success/failure without deserializing a response body
    /// </summary>
    protected async Task<ApiResult<string>> ExecutePostAsync<TRequest>(
        string endpoint,
        TRequest request,
        string operationName,
        string successMessage,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("{Operation}: posting to {Endpoint}", operationName, endpoint);
            
            var json = JsonSerializer.Serialize(request, JsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await HttpClient.PostAsync(endpoint, content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                Logger.LogInformation("{Operation} completed successfully", operationName);
                return ApiResult<string>.Success(successMessage);
            }
            
            return await HandleErrorResponse<string>(response, operationName, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            Logger.LogInformation("{Operation} was cancelled", operationName);
            return ApiResult<string>.Failure("Request was cancelled", 499);
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError(ex, "Network error during {Operation}", operationName);
            return ApiResult<string>.Failure("Network error: Unable to connect to API");
        }
        catch (TaskCanceledException ex)
        {
            Logger.LogError(ex, "Request timeout during {Operation}", operationName);
            return ApiResult<string>.Failure("Request timeout");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error during {Operation}", operationName);
            return ApiResult<string>.Failure($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes a POST request with form data and deserializes the response
    /// </summary>
    protected async Task<ApiResult<T>> ExecutePostFormAsync<T>(
        string endpoint,
        MultipartFormDataContent formData,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("{Operation}: posting form to {Endpoint}", operationName, endpoint);
            
            var response = await HttpClient.PostAsync(endpoint, formData, cancellationToken);
            
            return await HandleResponse<T>(response, operationName, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            Logger.LogInformation("{Operation} was cancelled", operationName);
            return ApiResult<T>.Failure("Request was cancelled", 499);
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError(ex, "Network error during {Operation}", operationName);
            return ApiResult<T>.Failure("Network error: Unable to connect to API");
        }
        catch (TaskCanceledException ex)
        {
            Logger.LogError(ex, "Request timeout during {Operation}", operationName);
            return ApiResult<T>.Failure("Request timeout");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error during {Operation}", operationName);
            return ApiResult<T>.Failure($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes a PUT request with JSON body and deserializes the response
    /// </summary>
    protected async Task<ApiResult<T>> ExecutePutAsync<T, TRequest>(
        string endpoint,
        TRequest request,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("{Operation}: putting to {Endpoint}", operationName, endpoint);
            
            var response = await HttpClient.PutAsJsonAsync(endpoint, request, JsonOptions, cancellationToken);
            
            return await HandleResponse<T>(response, operationName, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            Logger.LogInformation("{Operation} was cancelled", operationName);
            return ApiResult<T>.Failure("Request was cancelled", 499);
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError(ex, "Network error during {Operation}", operationName);
            return ApiResult<T>.Failure("Network error: Unable to connect to API");
        }
        catch (TaskCanceledException ex)
        {
            Logger.LogError(ex, "Request timeout during {Operation}", operationName);
            return ApiResult<T>.Failure("Request timeout");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error during {Operation}", operationName);
            return ApiResult<T>.Failure($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes a DELETE request
    /// </summary>
    protected async Task<ApiResult<bool>> ExecuteDeleteAsync(
        string endpoint,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("{Operation}: deleting at {Endpoint}", operationName, endpoint);
            
            var response = await HttpClient.DeleteAsync(endpoint, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                Logger.LogInformation("{Operation} completed successfully", operationName);
                return ApiResult<bool>.Success(true);
            }
            
            return await HandleErrorResponse<bool>(response, operationName, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            Logger.LogInformation("{Operation} was cancelled", operationName);
            return ApiResult<bool>.Failure("Request was cancelled", 499);
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError(ex, "Network error during {Operation}", operationName);
            return ApiResult<bool>.Failure("Network error: Unable to connect to API");
        }
        catch (TaskCanceledException ex)
        {
            Logger.LogError(ex, "Request timeout during {Operation}", operationName);
            return ApiResult<bool>.Failure("Request timeout");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error during {Operation}", operationName);
            return ApiResult<bool>.Failure($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles a successful or failed HTTP response
    /// </summary>
    private async Task<ApiResult<T>> HandleResponse<T>(
        HttpResponseMessage response,
        string operationName,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
            
            if (result != null)
            {
                Logger.LogInformation("{Operation} completed successfully", operationName);
                return ApiResult<T>.Success(result);
            }
            
            return ApiResult<T>.Failure("Failed to deserialize response");
        }
        
        return await HandleErrorResponse<T>(response, operationName, cancellationToken);
    }

    /// <summary>
    /// Handles an error HTTP response
    /// </summary>
    private async Task<ApiResult<T>> HandleErrorResponse<T>(
        HttpResponseMessage response,
        string operationName,
        CancellationToken cancellationToken)
    {
        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
        Logger.LogError("{Operation} failed. Status: {StatusCode}, Content: {Content}",
            operationName, response.StatusCode, errorContent);
        
        var errorMessage = TryParseErrorMessage(errorContent) 
            ?? $"API request failed with status {response.StatusCode}";
        
        return ApiResult<T>.Failure(errorMessage, (int)response.StatusCode);
    }

    /// <summary>
    /// Attempts to parse an error message from the response content
    /// </summary>
    protected string? TryParseErrorMessage(string content)
    {
        try
        {
            var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, JsonOptions);
            return errorResponse?.Error;
        }
        catch
        {
            return null;
        }
    }
}
