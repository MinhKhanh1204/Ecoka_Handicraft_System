namespace FeedbackAPI.Services
{
    public interface IOrderService
    {
        Task<bool> HasPurchasedAsync(string productId);
    }
}
