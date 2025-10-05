namespace CreditCalculatorApi.Messaging
{
    public class KafkaOptions
    {
        public string BootstrapServers { get; set; } = default!;
        public string ClientId { get; set; } = "creditcalc-api";
        public TopicsConfig Topics { get; set; } = new();

        public class TopicsConfig
        {
            public string CreditApplicationCreated { get; set; } = "creditapp.created";
            public string CreditApplicationStatusChanged { get; set; } = "creditapp.status.changed";
            public string RiskEvaluated { get; set; } = "risk.evaluated";
            public string DecisionMade { get; set; } = "decision.made";
            public string AppLogs { get; set; } = "app.logs";

        }
    }
}
