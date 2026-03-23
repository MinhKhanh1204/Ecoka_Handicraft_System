using ChatbotAPI.DTOs;

namespace ChatbotAPI.Services
{
    public interface IPromptBuilderService
    {
        string BuildSystemPrompt(ConversationContextDto context, string? customerName);
    }
}