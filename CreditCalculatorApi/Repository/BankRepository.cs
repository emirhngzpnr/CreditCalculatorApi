using CreditCalculatorApi.Data;
using CreditCalculatorApi.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CreditCalculatorApi.Repository
{
    public class BankRepository:IBankRepository
    {
        private readonly ApplicationDbContext _context;

        public BankRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetBankIdByNameAsync(string bankName)
        {
            var bank = await _context.Banks.FirstOrDefaultAsync(b => b.Name.ToLower() == bankName.ToLower());

            if (bank == null)
                throw new Exception($"Banka bulunamadı: {bankName}");

            return bank.Id;
        }
    }
}
