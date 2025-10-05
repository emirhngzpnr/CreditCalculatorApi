using CreditCalculatorApi.Entities.Enums;

namespace CreditCalculatorApi.Entities
{
    public class DecisionNotification
    {
        public Guid Id { get; set; }
        public int ApplicationId { get; set; }
        public DecisionOutcome Decision { get; set; }
        public DateTime SentAtUtc { get; set; }
    }
}
