using Confluent.Kafka;
using CreditCalculatorApi.Data;
using CreditCalculatorApi.Entities;
using CreditCalculatorApi.Events;
using CreditCalculatorApi.Messaging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace CreditCalculatorApi.BackgroundServices
{
    public class LogToSqlConsumer:BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IServiceScopeFactory _scopeFactory;
        private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

        public LogToSqlConsumer(IOptions<KafkaOptions> opt, IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
            {
                BootstrapServers = opt.Value.BootstrapServers,
                GroupId = "app-logs-sql",
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
                    if (evt != null)
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        db.AppLogs.Add(new LogModel
                        {
                            LogType = evt.LogType,
                            Message = evt.Message,
                            UserId = evt.UserId,
                            Source = evt.Source,
                            CreatedAt = evt.CreatedAtUtc,
                            Data = evt.Data?.ToString(),
                            Exception = evt.Exception
                        });
                        await db.SaveChangesAsync(ct);
                    }
                    _consumer.Commit(cr);
                }
                catch (OperationCanceledException) { }
                catch (Exception) {  }
            }
        }, ct);
    }
}
