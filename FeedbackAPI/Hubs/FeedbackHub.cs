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

        // ===== Feedback-level groups (reply events) =====

        public async Task JoinFeedbackGroup(int feedbackId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"feedback_{feedbackId}");
        }

        public async Task LeaveFeedbackGroup(int feedbackId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"feedback_{feedbackId}");
        }
    }
}
