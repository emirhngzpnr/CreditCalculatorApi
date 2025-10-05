using CreditCalculatorApi.Entities;

namespace CreditCalculatorApi.Services.Interfaces
{
    public interface ILogService
    {
        Task LogAsync(LogModel model);
    }
}
