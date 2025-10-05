using CreditCalculatorApi.Entities;

namespace CreditCalculatorApi.Repository.Interfaces
{
    public interface ICustomerApplicationRepository
    {
        Task<List<CustomerApplication>> GetAllAsync();
        Task AddAsync(CustomerApplication application);
        Task<CustomerApplication?> GetByIdentityAndBankAsync(string identityNumber, string bankName);
        Task<CustomerApplication?> GetByIdAsync(int id);
        Task DeleteAsync(CustomerApplication entity);
        Task<bool> IsCustomerAsync(string email, string bankName);
        Task<bool> CustomerNumberExistsAsync(string customerNumber);
        Task<List<CustomerApplication>> GetByUserIdAsync(string userId);

        Task SaveChangesAsync();


    }
}
