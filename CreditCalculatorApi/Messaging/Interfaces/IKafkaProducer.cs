namespace CreditCalculatorApi.Messaging.Interfaces
{
    public interface IKafkaProducer
    {
        Task ProduceAsync<T>(string topic, T message, string? key = null,
                                         IEnumerable<KeyValuePair<string, string>>? headers = null,
                                         CancellationToken ct = default);

    }
}
