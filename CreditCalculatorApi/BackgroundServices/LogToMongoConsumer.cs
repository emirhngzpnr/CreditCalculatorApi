using Confluent.Kafka;
using CreditCalculatorApi.Events;
using CreditCalculatorApi.Messaging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace CreditCalculatorApi.BackgroundServices
{
    public class LogToMongoConsumer:BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly MongoReadDb _mongo;
        private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

        public LogToMongoConsumer(IOptions<KafkaOptions> opt, MongoReadDb mongo)
        {
            _mongo = mongo;
            _consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
            {
                BootstrapServers = opt.Value.BootstrapServers,
                GroupId = "app-logs-mongo",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            }).Build();
            _consumer.Subscribe(opt.Value.Topics.AppLogs);
        }

        protected override Task ExecuteAsync(CancellationToken ct) => Task.Run(async () =>
        {
            while (!ct.IsCancellationRequested)
            {
                ConsumeResult<string, string>? cr = null;
                try
                {
                    cr = _consumer.Consume(ct);
                    var evt = JsonSerializer.Deserialize<AppLogEvent>(cr.Message.Value, JsonOpts);
                    if (evt != null) await _mongo.Logs.InsertOneAsync(evt, cancellationToken: ct);
                    _consumer.Commit(cr);
                }
                catch (OperationCanceledException) { }
                catch (Exception) { }
            }
        }, ct);
    }
}
