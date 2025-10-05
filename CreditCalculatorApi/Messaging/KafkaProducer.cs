using Confluent.Kafka;
using System.Text.Json;
using Microsoft.Extensions.Options;
using CreditCalculatorApi.Messaging.Interfaces;

namespace CreditCalculatorApi.Messaging
{
    public class KafkaProducer:IKafkaProducer,
        IDisposable
    {
        private readonly IProducer<string, string> _producer;

        public KafkaProducer(IOptions<KafkaOptions> options)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = options.Value.BootstrapServers,
                ClientId = options.Value.ClientId,
                Acks = Acks.All,
                EnableIdempotence = true,
                MessageSendMaxRetries = 5,
                LingerMs = 10,             
                BatchSize = 64 * 1024,       
                CompressionType = CompressionType.Zstd,
                MessageTimeoutMs = 30000,
                SocketTimeoutMs = 30000
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
        public async Task ProduceAsync<T>(string topic, T message, string? key = null,
                                        IEnumerable<KeyValuePair<string, string>>? headers = null,
                                        CancellationToken ct = default)
        {
            var payload = JsonSerializer.Serialize(message, JsonOpts);

            var msg = new Message<string, string>
            {
                Key = key ?? Guid.NewGuid().ToString(),
                Value = payload,
                Headers = new Headers()
            };
            if (headers != null)
                foreach (var h in headers) msg.Headers!.Add(h.Key, System.Text.Encoding.UTF8.GetBytes(h.Value));

            DeliveryResult<string, string> result = await _producer.ProduceAsync(topic, msg, ct);
           
        }


        public void Dispose() => _producer.Flush(TimeSpan.FromSeconds(2));

    }
}
