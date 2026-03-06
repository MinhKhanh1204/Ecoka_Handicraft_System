namespace FeedbackAPI.Services
{

    public interface IAccountService
    {
        Task<string?> GetUsernameAsync(string customerId);
    }
}
