using System;
using CreditCalculatorApi.Data;
using CreditCalculatorApi.Entities;
using CreditCalculatorApi.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CreditCalculatorApi.Repository
{
    public class CampaignRepository:ICampaignRepository
    {
        private readonly ApplicationDbContext _context;
        public CampaignRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<Campaign>> GetAllWithBanksAsync()
        {
            return await _context.Campaigns.Include(c => c.Bank).ToListAsync();
        }
        public async Task AddAsync(Campaign campaign)
        {
            await _context.Campaigns.AddAsync(campaign);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Campaign campaign)
        {
            _context.Campaigns.Update(campaign);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(Campaign campaign)
        {
            _context.Campaigns.Remove(campaign);
            await _context.SaveChangesAsync();
        }
        public async Task<Campaign?> GetByIdAsync(int id)
        {
            return await _context.Campaigns
                .Include(c => c.Bank) // güncelleme veya silme işleminde Bank bilgisi lazımsa
                .FirstOrDefaultAsync(c => c.Id == id);
        }
        public async Task<List<Campaign>> GetActiveCampaignsWithBanksAsync()
        {
            return await _context.Campaigns
                .Include(c => c.Bank)
                .Where(c => c.BitisTarihi >= DateTime.Now &&
                            c.BaslangicTarihi<=DateTime.Now)
                .ToListAsync();
        }
        public async Task<List<Campaign>> GetAllAsync()
        {
            return await _context.Campaigns.Include(c => c.Bank).ToListAsync();
        }


    }
}