using Confluent.Kafka;
using CreditCalculatorApi.Events;
using CreditCalculatorApi.Messaging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CreditCalculatorApi.BackgroundServices
{
    public class CreditAppCreatedConsumer : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly MongoReadDb _mongo;
        private readonly string _topic;
        private readonly ILogger<CreditAppCreatedConsumer> _logger;

        public CreditAppCreatedConsumer(
            IOptions<KafkaOptions> kafkaOptions,
            MongoReadDb mongo,
            ILogger<CreditAppCreatedConsumer> logger)
        {
            _logger = logger;
            _mongo = mongo;

            var config = new ConsumerConfig
            {
                BootstrapServers = kafkaOptions.Value.BootstrapServers,
                GroupId = "creditapp-consumer-group", 
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true
            };

            _consumer = new ConsumerBuilder<string, string>(config).Build();
            _topic = kafkaOptions.Value.Topics.CreditApplicationCreated;
        }

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(_topic);
            return Task.Run(() =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var cr = _consumer.Consume(stoppingToken);
                        var message = JsonSerializer.Deserialize<CreditApplicationCreated>(cr.Message.Value, JsonOpts);

                        if (message != null)
                        {
                            _mongo.CreditApps.InsertOne(message); 
                            _logger.LogInformation("[Consumer] Mongo'ya eklendi: {FullName}", message.FullName);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[Consumer] Hata oluştu");
                    }
                }
            }, stoppingToken);
        }

        public override void Dispose()
        {
            _consumer?.Close();
            _consumer?.Dispose();
            base.Dispose();
        }
    }
}
