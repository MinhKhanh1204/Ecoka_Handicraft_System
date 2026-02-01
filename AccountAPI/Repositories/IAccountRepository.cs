using AccountAPI.Models;

namespace AccountAPI.Repositories
{
    public interface IAccountRepository
    {
        Account? GetByUsername(string username);
    }
}
