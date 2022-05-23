﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApplication.Infrastructure.Contexts;
using WebApplication.Infrastructure.Entities;
using WebApplication.Infrastructure.Interfaces;

namespace WebApplication.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly InMemoryContext _dbContext;

        public UserService(InMemoryContext dbContext)
        {
            _dbContext = dbContext;

            // this is a hack to seed data into the in memory database. Do not use this in production.
            _dbContext.Database.EnsureCreated();
        }

        /// <inheritdoc />
        public async Task<User?> GetAsync(int id, CancellationToken cancellationToken = default)
        {
            User? user = await _dbContext.Users.Where(user => user.Id == id)
                                         .Include(x => x.ContactDetail)
                                         .FirstOrDefaultAsync(cancellationToken);

            return user;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<User>> FindAsync(string? givenNames, string? lastName, CancellationToken cancellationToken = default)
        {
            var result = _dbContext.Users.Where(x => x.GivenNames.ToLower() == givenNames.ToLower() || 
                                                     x.LastName.ToLower() == lastName.ToLower())
                                         .Include(y => y.ContactDetail).ToList();
            return result;

            //throw new NotImplementedException("Implement a way to find users that match the provided given names OR last name.");
        }

        /// <inheritdoc />
        public async Task<IEnumerable<User>> GetPaginatedAsync(int page, int count, CancellationToken cancellationToken = default)
        {
            
            var result = await _dbContext.Users.Skip((page - 1) * count).Take(count).ToListAsync(cancellationToken);
            return result;

            //throw new NotImplementedException("Implement a way to get a 'page' of users.");
        }

        /// <inheritdoc />
        public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
        {   
            var result = await _dbContext.Users.AddAsync(user);
            _dbContext.SaveChanges();
            return result.Entity;

            //throw new NotImplementedException("Implement a way to add a new user, including their contact details.");
        }

        /// <inheritdoc />
        public async Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            var result = _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
            return result.Entity;

            //throw new NotImplementedException("Implement a way to update an existing user, including their contact details.");
        }

        /// <inheritdoc />
        public async Task<User?> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
                return null;

            var result = _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
            return result.Entity;

            //throw new NotImplementedException("Implement a way to delete an existing user, including their contact details.");
        }

        /// <inheritdoc />
        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            var result = _dbContext.Users.Count();
            return result;            //throw new NotImplementedException("Implement a way to count the number of users in the database.");

        }
    }
}