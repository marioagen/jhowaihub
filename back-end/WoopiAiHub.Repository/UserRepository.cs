using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
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
        /// <param name="user"></param>
        /// <returns></returns>
        public bool Create(User user)
        {
            var existUser = _context.Users.Any(p => p.Email == user.Email && p.IsActive == false);
            if (!existUser)
            {
                _context.Users.Add(user);
                _context.SaveChanges();

                return true;
            }
            return false;
        }

        /// <summary>
        /// Find users by ids and convert to a User list
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
        /// Delete users by ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool DeactivateRange(List<Guid> ids)
        {

            var usersInDb = _context.Users
                .Where(u => ids.Contains(u.Id))
                .ToList();

                foreach (var user in usersInDb)
                {
                    user.IsActive = false;
                }

                _context.Users.UpdateRange(usersInDb);
                _context.SaveChangesAsync();

                return true;

        }

        /// <summary>
        /// Update an user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool Update(User user)
        {
            var existing = _context.Users
                .Include(u => u.Teams)
                .FirstOrDefault(u => u.Id == user.Id);

            if (existing == null)
                return false;

            existing.Name = user.Name;
            existing.Email = user.Email;
            existing.IsActive = user.IsActive;

            existing.Teams.Clear();
            foreach (var team in user.Teams)
            {
                if (_context.Entry(team).State == EntityState.Detached)
                    _context.Teams.Attach(team);

                existing.Teams.Add(team);
            }

            _context.Users.Update(existing);
            _context.SaveChanges();
            return true;
        }

        /// <summary>
        /// Find all teams with pagination and include their users.
        /// </summary>
        /// <param name="pagedDataDto"></param>
        /// <returns></returns>
        public IQueryable<UserDtoPaged> FindAllPaged(PagedDataDto pagedDataDto)
        {
            var query = _context.Users.Where(p=> p.IsActive == true)
                .Include(t => t.Teams)
                .Select(t => new UserDtoPaged
                {
                    Id = t.Id,
                    Name = t.Name,
                    Created = t.Created,
                    Email = t.Email,
                    IsActive = t.IsActive,
                    Teams = t.Teams!
                        .Select(u => new TeamDto
                        {
                            Id = u.Id,
                            Name = u.Name,
                            Created = u.Created
                        })
                        .ToList()
                })
                .AsQueryable()
                .AsNoTracking();

            return query;
        }

    }
}
