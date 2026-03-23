namespace ChatbotAPI.Exceptions;

/// <summary>
/// Thrown when Google Generative Language API returns HTTP 429 (quota / rate limit).
/// </summary>
public sealed class GeminiRateLimitException : Exception
{
    public GeminiRateLimitException(string message) : base(message) { }
}
