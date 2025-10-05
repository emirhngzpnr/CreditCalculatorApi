using CreditCalculatorApi.Entities;

namespace CreditCalculatorApi.Repository.Interfaces
{
    public interface ICreditCalculationRepository
    {
        Task AddAsync(CreditCalculation entity);
        Task<List<CreditCalculation>> GetAllAsync();
        Task<List<CreditCalculation>> FilterAsync(decimal? minFaiz, decimal? maxFaiz, int? vade);
    }
}
