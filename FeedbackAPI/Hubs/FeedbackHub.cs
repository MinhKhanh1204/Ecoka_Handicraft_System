using Microsoft.AspNetCore.SignalR;

namespace FeedbackAPI.Hubs
{
    public class FeedbackHub : Hub
    {
        // ===== Product-level groups (feedback events) =====

        public async Task JoinProductGroup(string productId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"product_{productId}");
        }

        public async Task LeaveProductGroup(string productId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"product_{productId}");
        }

        
    }
}
