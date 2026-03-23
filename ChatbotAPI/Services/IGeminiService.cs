using ChatbotAPI.DTOs;

namespace ChatbotAPI.Services;

public interface IGeminiService
{
    Task<string> GenerateResponseAsync(string userMessage, string systemPrompt);

    /// <summary>Gọi thử Gemini với prompt tối thiểu — dùng cho diagnostics, không ném exception ra ngoài.</summary>
    Task<GeminiConnectionTestDto> TestConnectionAsync(CancellationToken cancellationToken = default);
}
