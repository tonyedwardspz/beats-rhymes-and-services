using SharedLibrary.Models;

namespace App.Services;

/// <summary>
/// Service for communicating with the LLM API
/// </summary>
public class LLMApiService : BaseApiService, ILLMApiService
{
    public LLMApiService(
        HttpClient httpClient, 
        ILogger<LLMApiService> logger,
        IOptions<ApiConfiguration> config)
        : base(httpClient, logger)
    {
        var configValue = config.Value;
        HttpClient.BaseAddress = new Uri(configValue.BaseUrl);
        HttpClient.Timeout = configValue.Timeout;
    }

    /// <inheritdoc />
    public async Task<ApiResult<ModelInfoResponse>> GetModelInfoAsync()
    {
        return await ExecuteGetAsync<ModelInfoResponse>(
            "/api/llm/info",
            "GetModelInfo");
    }

    /// <inheritdoc />
    public async Task<ApiResult<GenerateResponse>> GenerateResponseAsync(
        string prompt, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return ApiResult<GenerateResponse>.Failure("Prompt cannot be empty", 400);
        }

        Logger.LogInformation("Generating response for prompt (length: {Length})", prompt.Length);
        
        return await ExecutePostAsync<GenerateResponse, GenerateRequest>(
            "/api/llm/generate",
            new GenerateRequest(prompt),
            "GenerateResponse",
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<string>> GenerateStreamingResponseAsync(
        string prompt, 
        Action<string> onTokenReceived, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return ApiResult<string>.Failure("Prompt cannot be empty", 400);
        }

        ArgumentNullException.ThrowIfNull(onTokenReceived);

        try
        {
            Logger.LogInformation("Starting streaming response for prompt (length: {Length})", prompt.Length);
            
            var request = new GenerateRequest(prompt);
            var jsonContent = JsonSerializer.Serialize(request, JsonOptions);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            
            using var response = await HttpClient.PostAsync("/api/llm/stream", content, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                Logger.LogError("Failed to start streaming response. Status: {StatusCode}, Content: {Content}", 
                    response.StatusCode, errorContent);
                
                var errorMessage = TryParseErrorMessage(errorContent) 
                    ?? $"LLMAPI request failed with status {response.StatusCode}";
                
                return ApiResult<string>.Failure(errorMessage, (int)response.StatusCode);
            }
            
            var fullResponse = new StringBuilder();
            
            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);
            
            var buffer = new char[1024];
            int bytesRead;
            
            while ((bytesRead = await reader.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
            {
                var token = new string(buffer, 0, bytesRead);
                fullResponse.Append(token);
                onTokenReceived(token);
            }
            
            var finalResponse = fullResponse.ToString();
            Logger.LogInformation("Completed streaming response (length: {Length})", finalResponse.Length);
            
            return ApiResult<string>.Success(finalResponse);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            Logger.LogInformation("Streaming response request was cancelled");
            return ApiResult<string>.Failure("Request was cancelled", 499);
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError(ex, "Network error while streaming response");
            return ApiResult<string>.Failure("Network error: Unable to connect to API");
        }
        catch (TaskCanceledException ex)
        {
            Logger.LogError(ex, "Request timeout while streaming response");
            return ApiResult<string>.Failure("Request timeout");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error while streaming response");
            return ApiResult<string>.Failure($"Unexpected error: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<ApiResult<LLMConfigurationResponse>> GetConfigurationAsync(CancellationToken cancellationToken = default)
    {
        return await ExecuteGetAsync<LLMConfigurationResponse>(
            "/api/llm/config",
            "GetConfiguration",
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<LLMConfigurationResponse>> UpdateConfigurationAsync(
        LLMConfigurationUpdateRequest request, 
        CancellationToken cancellationToken = default)
    {
        return await ExecutePutAsync<LLMConfigurationResponse, LLMConfigurationUpdateRequest>(
            "/api/llm/config",
            request,
            "UpdateConfiguration",
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<CreateChatSessionResponse>> CreateChatSessionAsync(CancellationToken cancellationToken = default)
    {
        return await ExecutePostAsync<CreateChatSessionResponse, object?>(
            "/api/llm/chat/create",
            null,
            "CreateChatSession",
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<ChatMessageResponse>> SendChatMessageAsync(
        string sessionId, 
        string message, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return ApiResult<ChatMessageResponse>.Failure("Session ID cannot be empty", 400);
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            return ApiResult<ChatMessageResponse>.Failure("Message cannot be empty", 400);
        }

        Logger.LogInformation("Sending chat message to session {SessionId} (length: {Length})", sessionId, message.Length);
        
        return await ExecutePostAsync<ChatMessageResponse, ChatMessageRequest>(
            $"/api/llm/chat/{sessionId}",
            new ChatMessageRequest(message),
            "SendChatMessage",
            cancellationToken);
    }
}
