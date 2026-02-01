using AccountAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountAPI.Repositories.Implements
{
    public class AccountRepository : IAccountRepository
    {
        private readonly DBContext _context;

        public AccountRepository(DBContext context)
        {
            _context = context;
        }
        public Account? GetByUsername(string username)
        {
            return _context.Accounts
                .Include(a => a.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefault(a => a.Username == username);
        }

    }
}
