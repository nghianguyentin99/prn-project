using PCShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCShop.Repository
{
    public class UserRepository
    {
        private readonly PcshopDbContext _context;

        public UserRepository()
        {
            _context = new PcshopDbContext();
        }

        public User? Login(string username, string password)
        {
            return _context.Users
                .FirstOrDefault(u => u.Username == username && u.Password == password);
        }
    }
}
