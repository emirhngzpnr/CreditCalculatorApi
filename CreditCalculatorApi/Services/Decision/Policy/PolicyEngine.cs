using CreditCalculatorApi.Data;
using CreditCalculatorApi.Entities.Enums;
using System.Text.Json;
using CreditCalculatorApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CreditCalculatorApi.Services.Decision.Policy
{
    public class PolicyEngine : IPolicyEngine
    {
        private readonly ApplicationDbContext _db;
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public PolicyEngine(ApplicationDbContext db) => _db = db;

        public Task<(DecisionOutcome outcome, string ruleId, string version, string reason)>
            DecideAsync(decimal dti, string riskLabel, decimal amount, int term, int? campaignId, int bankId /* ignored */)
        {
            var risk = riskLabel?.Trim() ?? string.Empty;

            if (risk.Equals("Safe", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult((
                    DecisionOutcome.Approved,
                    "RULE_RISK_SAFE",
                    "policy-simple",
                    "Risk=Safe → Approved"
                ));
            }

            if (risk.Equals("Risky", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult((
                    DecisionOutcome.Declined,
                    "RULE_RISK_RISKY",
                    "policy-simple",
                    "Risk=Risky → Declined"
                ));
            }

            // Varsayılan: Medium veya tanımsız risk → ManualReview
            return Task.FromResult((
                DecisionOutcome.ManualReview,
                "RULE_RISK_OTHER",
                "policy-simple",
                $"Risk={risk} → ManualReview"
            ));
        }

        private class Cond
        {
            public string? Risk { get; set; }
            public decimal? DtiMax { get; set; }
            public decimal? DtiMin { get; set; }
            public int? TermMax { get; set; }
            public int? TermMin { get; set; }
            public decimal? AmountMax { get; set; }
            public decimal? AmountMin { get; set; }
            public int? CampaignId { get; set; }
        }
    }
}
