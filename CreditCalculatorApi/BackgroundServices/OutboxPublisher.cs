using Confluent.Kafka;
using CreditCalculatorApi.Data;
using CreditCalculatorApi.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using CreditCalculatorApi.Messaging;

namespace CreditCalculatorApi.BackgroundServices
{
    public class OutboxPublisher : BackgroundService
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<OutboxPublisher> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string _topic;

        public OutboxPublisher(
            IOptions<KafkaOptions> kafkaOptions,
            IProducer<string, string> producer,
            IServiceScopeFactory scopeFactory,
            ILogger<OutboxPublisher> logger)
        {
            _producer = producer;
            _scopeFactory = scopeFactory;
            _logger = logger;
            _topic = kafkaOptions.Value.Topics.DecisionMade; 
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var batch = await db.Set<OutboxMessage>()
                        .Where(x => x.ProcessedOnUtc == null)
                        .OrderBy(x => x.Id)
                        .Take(100)
                        .ToListAsync(stoppingToken);

                    foreach (var msg in batch)
                    {//!!
                        try
                        {
                            await _producer.ProduceAsync(_topic, new Message<string, string>
                            {
                                Key = msg.AggregateId,
                                Value = msg.Payload
                            }, stoppingToken);

                            msg.ProcessedOnUtc = DateTime.UtcNow;
                        }
                        catch (Exception ex)
                        {
                            msg.Error = ex.Message;
                            _logger.LogError(ex, "Outbox publish failed for Id={Id}", msg.Id);
                        }
                    }

                    await db.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "OutboxPublisher loop error");
                }

                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }
    }
}
