namespace CreditCalculatorApi.ReadModels
{
    public class MongoOptions
    {
        public string ConnectionString { get; set; } = default!;
        public string Database { get; set; } = default!;
        public CollectionsConfig Collections { get; set; } = new();
        public class CollectionsConfig
        {
            public string Events { get; set; } = "DomainEvents";
            public string CreditApps { get; set; } = "CreditApplicationsRead";
            public string Logs { get; set; } = "Logs";
        }
    }
}
