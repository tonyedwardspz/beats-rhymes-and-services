namespace App.Helpers;

/// <summary>
/// Helper class for file operations
/// </summary>
public static class FileHelper
{
    /// <summary>
    /// Known audio file extensions and their corresponding MIME types
    /// </summary>
    private static readonly Dictionary<string, string> AudioMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".wav", "audio/wav" },
        { ".mp3", "audio/mpeg" },
        { ".m4a", "audio/mp4" },
        { ".aac", "audio/aac" },
        { ".ogg", "audio/ogg" },
        { ".flac", "audio/flac" },
        { ".webm", "audio/webm" },
        { ".wma", "audio/x-ms-wma" }
    };

    /// <summary>
    /// Gets the MIME content type for an audio file based on its extension
    /// </summary>
    /// <param name="filePath">The file path or file name</param>
    /// <param name="defaultContentType">The default content type to return if extension is not recognized</param>
    /// <returns>The MIME content type string</returns>
    public static string GetAudioContentType(string filePath, string defaultContentType = "audio/wav")
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return defaultContentType;
        }

        var extension = Path.GetExtension(filePath);
        
        if (string.IsNullOrEmpty(extension))
        {
            return defaultContentType;
        }

        return AudioMimeTypes.TryGetValue(extension, out var mimeType) 
            ? mimeType 
            : defaultContentType;
    }

    /// <summary>
    /// Validates that a file path is not null/empty and the file exists
    /// </summary>
    /// <param name="filePath">The file path to validate</param>
    /// <returns>A tuple containing (isValid, errorMessage). errorMessage is null if valid.</returns>
    public static (bool IsValid, string? ErrorMessage, int? ErrorCode) ValidateFilePath(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return (false, "File path cannot be null or empty", 400);
        }

        if (!File.Exists(filePath))
        {
            return (false, $"File not found: {filePath}", 404);
        }

        return (true, null, null);
    }

    /// <summary>
    /// Gets the size of a file in bytes
    /// </summary>
    /// <param name="filePath">The path to the file</param>
    /// <returns>The file size in bytes, or -1 if the file doesn't exist</returns>
    public static long GetFileSize(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return -1;
        }

        return new FileInfo(filePath).Length;
    }

    /// <summary>
    /// Checks if a file extension represents an audio file
    /// </summary>
    /// <param name="filePath">The file path or file name to check</param>
    /// <returns>True if the file has a recognized audio extension</returns>
    public static bool IsAudioFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return false;
        }

        var extension = Path.GetExtension(filePath);
        return !string.IsNullOrEmpty(extension) && AudioMimeTypes.ContainsKey(extension);
    }

    /// <summary>
    /// Gets all supported audio file extensions
    /// </summary>
    /// <returns>A collection of supported audio extensions (including the dot)</returns>
    public static IEnumerable<string> GetSupportedAudioExtensions()
    {
        return AudioMimeTypes.Keys;
    }
}
