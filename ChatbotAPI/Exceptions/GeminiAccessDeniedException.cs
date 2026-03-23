namespace ChatbotAPI.Exceptions;

/// <summary>
/// Google trả 401/403 (key bị khóa, CONSUMER_SUSPENDED, API không bật, v.v.).
/// </summary>
public sealed class GeminiAccessDeniedException : Exception
{
    public GeminiAccessDeniedException(string message) : base(message) { }
}
