using ChatbotAPI.DTOs;

namespace ChatbotAPI.Services
{
    public interface IContextBuilderService
    {
        Task<ConversationContextDto> BuildContextAsync(string? customerId);
    }
}