using System.Threading.Tasks;

namespace MVCApplication.Services
{
    public interface IPaymentService
    {
        Task<string?> GetVnPayUrlAsync(string orderId, decimal amount, string orderInfo, string returnUrl);
        Task<string?> GetMomoUrlAsync(string orderId, decimal amount, string orderInfo, string returnUrl);
    }
}
