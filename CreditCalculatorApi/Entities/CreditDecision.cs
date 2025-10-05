using CreditCalculatorApi.Entities.Enums;

namespace CreditCalculatorApi.Entities
{
    public class CreditDecision
    {
        public Guid Id { get; set; }
        public int ApplicationId { get; set; }

        public DecisionOutcome Decision { get; set; }   // <-- enum
        public string RuleId { get; set; } = "";
        public string Reason { get; set; } = "";
        public string InputsJson { get; set; } = "";
        public string Version { get; set; } = "";
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
