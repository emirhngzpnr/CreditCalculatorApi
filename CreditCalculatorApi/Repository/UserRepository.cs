using CreditCalculatorApi.Entities;
using System;
using CreditCalculatorApi.Repository.Interfaces;
using CreditCalculatorApi.Data;
using Microsoft.EntityFrameworkCore;

namespace CreditCalculatorApi.Repository
{
    public class UserRepository:IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> AnyByUserNumberAsync(string userNumber)
        {
            return await _context.Users.AnyAsync(u => u.UserNumber == userNumber);
        }
        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}

