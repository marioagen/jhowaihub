using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly Context.ApplicationDbContext _context;
        public UserRepository(Context.ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Create an user in the database
        /// </summary>
        /// <param name="typeDoc"></param>
        /// <returns></returns>
        public bool Create(User user)
        {

            var existUser = _context.Users.Any(p => p.Name == user.Name);
            if (!existUser)
            {
                _context.Users.Add(user);
                _context.SaveChanges();

                return true;
            }
            return false;
        }
    }
}
