namespace CreditCalculatorApi.Entities
{
    public class DecisionPolicy
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int Priority { get; set; }
        public bool Enabled { get; set; } = true;
        public string ConditionsJson { get; set; } = "";
        public string Outcome { get; set; } = "";
        public string RuleId { get; set; } = "";
        public string Version { get; set; } = "";
        public DateTime EffectiveFromUtc { get; set; } = DateTime.UtcNow;
    }
}
