using CreditCalculatorApi.Data;
using CreditCalculatorApi.DTOs;
using CreditCalculatorApi.Entities;
using CreditCalculatorApi.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

public class CustomerApplicationRepository : ICustomerApplicationRepository
{
    private readonly ApplicationDbContext _context;

    public CustomerApplicationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerApplication>> GetAllAsync()
    {
        return await _context.CustomerApplications.ToListAsync();
    }

    public async Task AddAsync(CustomerApplication application)
    {
        await _context.CustomerApplications.AddAsync(application);
        await _context.SaveChangesAsync();
    }
    public async Task<CustomerApplication?> GetByIdentityAndBankAsync(string identityNumber, string bankName)
    {
        return await _context.CustomerApplications
            .FirstOrDefaultAsync(x =>
                x.IdentityNumber == identityNumber &&
                x.BankName.ToLower() == bankName.ToLower());
    }
    public async Task<CustomerApplication?> GetByIdAsync(int id)
    {
        return await _context.CustomerApplications.FindAsync(id);
    }
    public async Task DeleteAsync(CustomerApplication entity)
    {
        _context.CustomerApplications.Remove(entity);
        await _context.SaveChangesAsync();
    }
    public async Task<bool> IsCustomerAsync(string email, string bankName)
    {
        return await _context.CustomerApplications.AnyAsync(app =>
            app.Email == email &&
            app.BankName == bankName &&
            app.Status == CustomerApplicationStatus.Approved // sadece onaylananlar
        );
    }
    public async Task<bool> CustomerNumberExistsAsync(string customerNumber)
    {
        return await _context.CustomerApplications.AnyAsync(x => x.CustomerNumber == customerNumber);
    }
    public async Task<List<CustomerApplication>> GetByUserIdAsync(string userId)
    {
        return await _context.CustomerApplications
            .Where(app => app.UserId == userId)
            .OrderByDescending(app => app.ApplicationDate)
            .ToListAsync();
    }


    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

}
