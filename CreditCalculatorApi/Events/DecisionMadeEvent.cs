using System.Text.Json.Serialization;
using CreditCalculatorApi.Entities.Enums;
namespace CreditCalculatorApi.Events
{
    public class DecisionMadeEvent
    {
        public int ApplicationId { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DecisionOutcome Decision { get; set; }  // Approved | Declined | ManualReview
        public string RuleId { get; set; } = "";
        public string Reason { get; set; } = "";
        public object Inputs { get; set; } = default!;
        public string Version { get; set; } = "";
        public DateTime Ts { get; set; } = DateTime.UtcNow;
    }
}
