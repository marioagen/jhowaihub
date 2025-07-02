using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoopiAiHub.Domain.DTOs.Request;
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

        /// <summary>
        /// Find users by ids and convert to a Dto list
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<User> FindByIds(List<Guid> ids)
        {
            return _context.Users.Where(u => ids.Contains(u.Id))
                                       .AsNoTracking()
                                       .ToList();

        }

        /// <summary>
        /// Delete users
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool DeleteByIds(List<Guid> ids)
        {
            var users = _context.Users.Where(a => ids.Contains(a.Id));

            if (users.Count() > 0)
            {
                _context.Users.RemoveRange(users);
                _context.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Update an user
        /// </summary>
        /// <param name="userUpdateDto"></param>
        /// <returns></returns>
        public bool Update(UserUpdateDto userUpdateDto)
        {
            var existUser= _context.Users.Any(p => p.Email != userUpdateDto.Email);

            if (!existUser)
            {
                _context.Questions.Where(a => a.Id.Equals(userUpdateDto.Id))
                                  .ExecuteUpdate(b => b
                                  .SetProperty(u => userUpdateDto.Email, userUpdateDto.Email)
                                  .SetProperty(u => userUpdateDto.Name, userUpdateDto.Name)
                                  .SetProperty(u => userUpdateDto.Teams, userUpdateDto.Teams));

                _context.SaveChanges();

                return true;
            }
            return false;
        }
    }
}
