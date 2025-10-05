using CreditCalculatorApi.Entities.Enums;

namespace CreditCalculatorApi.Services.Interfaces
{
    public interface IPolicyEngine
    {
        Task<(DecisionOutcome outcome, string ruleId, string version, string reason)>
            DecideAsync(decimal dti, string riskLabel, decimal amount, int term, int? campaignId, int bankId);
    }
}
