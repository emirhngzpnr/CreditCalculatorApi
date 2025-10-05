using CreditCalculatorApi.Data;
using CreditCalculatorApi.Entities;
using CreditCalculatorApi.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CreditCalculatorApi.Repository
{
    public class CreditCalculationRepository : ICreditCalculationRepository
    {
        private readonly ApplicationDbContext _context;

        public CreditCalculationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(CreditCalculation entity)
        {
            await _context.CreditCalculations.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<List<CreditCalculation>> GetAllAsync()
        {
            return await _context.CreditCalculations
                .OrderByDescending(x => x.HesaplamaTarihi)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<CreditCalculation>> FilterAsync(decimal? minFaiz, decimal? maxFaiz, int? vade)
        {
            var query = _context.CreditCalculations.AsQueryable();

            if (minFaiz.HasValue)
                query = query.Where(x => x.FaizOrani >= minFaiz.Value);

            if (maxFaiz.HasValue)
                query = query.Where(x => x.FaizOrani <= maxFaiz.Value);

            if (vade.HasValue)
                query = query.Where(x => x.Vade == vade.Value);

            return await query
                .AsNoTracking()
                .OrderByDescending(x => x.HesaplamaTarihi)
                .ToListAsync();
        }

        /* 
         * Eğer IQueryable kullansaydık ...
        public IQueryable<CreditCalculation> FilterByFaiz(decimal min, decimal max)
{
    return _context.CreditCalculations
        .Where(x => x.FaizOrani >= min && x.FaizOrani <= max);
}

public IQueryable<CreditCalculation> FilterByVade(int vade)
{
    return _context.CreditCalculations
        .Where(x => x.Vade == vade);
}



         */
    }
}
