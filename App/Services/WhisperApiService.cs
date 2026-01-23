using App.Helpers;
using SharedLibrary.Models;

namespace App.Services;

/// <summary>
/// Service for communicating with the Whisper API
/// </summary>
public class WhisperApiService : BaseApiService, IWhisperApiService
{
    public WhisperApiService(
        HttpClient httpClient, 
        ILogger<WhisperApiService> logger,
        IOptions<ApiConfiguration> config)
        : base(httpClient, logger)
    {
        var configValue = config.Value;
        HttpClient.BaseAddress = new Uri(configValue.WhisperBaseURL);
        HttpClient.Timeout = configValue.Timeout;
    }

    /// <inheritdoc />
    public async Task<ApiResult<string>> GetModelDetailsAsync()
    {
        return await ExecuteGetStringAsync(
            "/api/whisper/modelDetails",
            "GetModelDetails");
    }

    /// <inheritdoc />
    public async Task<ApiResult<TranscriptionResponse>> TranscribeWavAsync(
        Stream audioStream, 
        string fileName, 
        string? transcriptionType = null,
        string? sessionId = null,
        int? chunkIndex = null,
        CancellationToken cancellationToken = default)
    {
        if (audioStream == null || audioStream.Length == 0)
        {
            return ApiResult<TranscriptionResponse>.Failure("Audio stream cannot be null or empty", 400);
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return ApiResult<TranscriptionResponse>.Failure("File name cannot be empty", 400);
        }

        Logger.LogInformation("Transcribing WAV file: {FileName} (size: {Size} bytes, chunkIndex: {ChunkIndex})", 
            fileName, audioStream.Length, chunkIndex);
        
        using var formData = new MultipartFormDataContent();
        
        var audioContent = new StreamContent(audioStream);
        audioContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/x-caf");
        
        formData.Add(audioContent, "File", fileName);
        AddOptionalTranscriptionFields(formData, transcriptionType, sessionId, chunkIndex);
        
        return await ExecutePostFormAsync<TranscriptionResponse>(
            "/api/whisper/transcribe-wav",
            formData,
            "TranscribeWav",
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<TranscriptionResponse>> TranscribeFileAsync(
        string filePath,
        string? transcriptionType = null,
        string? sessionId = null,
        CancellationToken cancellationToken = default)
    {
        var (isValid, errorMessage, errorCode) = FileHelper.ValidateFilePath(filePath);
        if (!isValid)
        {
            return ApiResult<TranscriptionResponse>.Failure(errorMessage!, errorCode ?? 400);
        }

        Logger.LogInformation("Transcribing file: {FilePath} (size: {Size} bytes)", 
            filePath, FileHelper.GetFileSize(filePath));
        
        using var formData = new MultipartFormDataContent();
        using var fileStream = File.OpenRead(filePath);
        
        var fileContent = new StreamContent(fileStream);
        var contentType = FileHelper.GetAudioContentType(filePath);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        
        formData.Add(fileContent, "File", Path.GetFileName(filePath));
        AddOptionalTranscriptionFields(formData, transcriptionType, sessionId);
        
        return await ExecutePostFormAsync<TranscriptionResponse>(
            "/api/whisper/transcribe-wav",
            formData,
            "TranscribeFile",
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<List<WhisperModel>>> GetAvailableModelsAsync(CancellationToken cancellationToken = default)
    {
        return await ExecuteGetAsync<List<WhisperModel>>(
            "/api/whisper/models",
            "GetAvailableModels",
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<string>> SwitchModelAsync(string modelName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(modelName))
        {
            return ApiResult<string>.Failure("Model name cannot be empty", 400);
        }

        return await ExecutePostAsync(
            "/api/whisper/models/switch",
            new { ModelName = modelName },
            "SwitchModel",
            $"Successfully switched to model: {modelName}",
            cancellationToken);
    }

    /// <summary>
    /// Adds optional transcription fields to the form data
    /// </summary>
    private static void AddOptionalTranscriptionFields(
        MultipartFormDataContent formData,
        string? transcriptionType,
        string? sessionId,
        int? chunkIndex = null)
    {
        if (!string.IsNullOrEmpty(transcriptionType))
        {
            formData.Add(new StringContent(transcriptionType), "TranscriptionType");
        }
        
        if (!string.IsNullOrEmpty(sessionId))
        {
            formData.Add(new StringContent(sessionId), "SessionId");
        }
        
        if (chunkIndex.HasValue)
        {
            formData.Add(new StringContent(chunkIndex.Value.ToString()), "ChunkIndex");
        }
    }
}
