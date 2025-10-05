using CreditCalculatorApi.Entities;

namespace CreditCalculatorApi.Services.Notification
{
    public interface INotificationService
    {
        Task SendApplicationReceivedAsync(CreditApplication app);
        Task SendStatusUpdateAsync(CreditApplication app);
        Task SendCustomerApplicationReceivedAsync(CustomerApplication app);
        Task SendCustomerStatusUpdateAsync(CustomerApplication app);

    }
}
