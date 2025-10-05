using CreditCalculatorApi.Entities;

namespace CreditCalculatorApi.Repository.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task AddAsync(User user);
        Task<bool> AnyByUserNumberAsync(string userNumber);
        Task UpdateAsync(User user);


    }
}
