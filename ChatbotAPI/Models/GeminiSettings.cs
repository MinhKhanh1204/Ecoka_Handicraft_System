namespace ChatbotAPI.Models;

public class GeminiSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-2.5-flash-lite";
    public int MaxTokens { get; set; } = 1024;
    public double Temperature { get; set; } = 0.7;
}
