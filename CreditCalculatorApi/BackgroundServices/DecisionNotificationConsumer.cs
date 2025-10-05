using Confluent.Kafka;
using CreditCalculatorApi.Data;
using CreditCalculatorApi.Events;
using CreditCalculatorApi.Messaging;
using CreditCalculatorApi.Services.Notification;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace CreditCalculatorApi.BackgroundServices
{
    public class DecisionNotificationConsumer : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IServiceScopeFactory _scopeFactory;
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public DecisionNotificationConsumer(IOptions<KafkaOptions> opt, IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
            {
                BootstrapServers = opt.Value.BootstrapServers,
                GroupId = "decision-notifier",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            }).Build();
            _consumer.Subscribe(opt.Value.Topics.DecisionMade);
        }

        protected override Task ExecuteAsync(CancellationToken ct) => Task.Run(async () =>
        {
            while (!ct.IsCancellationRequested)
            {
                ConsumeResult<string, string>? cr = null;
                try
                {
                    cr = _consumer.Consume(ct);
                    var evt = JsonSerializer.Deserialize<DecisionMadeEvent>(cr.Message.Value, JsonOpts);
                    if (evt is null) { _consumer.Commit(cr); continue; }

                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var notifier = scope.ServiceProvider.GetRequiredService<INotificationService>();

                
                    var alreadySent = await db.DecisionNotifications
                        .AnyAsync(x => x.ApplicationId == evt.ApplicationId && x.Decision == evt.Decision, ct);
                    if (!alreadySent)
                    {
                        var app = await db.CreditApplications.FindAsync(new object?[] { evt.ApplicationId }, ct);
                        if (app != null)
                        {
                            await notifier.SendStatusUpdateAsync(app);
                            db.DecisionNotifications.Add(new Entities.DecisionNotification
                            {
                                Id = Guid.NewGuid(),
                                ApplicationId = evt.ApplicationId,
                                Decision = evt.Decision,
                                SentAtUtc = DateTime.UtcNow
                            });
                            await db.SaveChangesAsync(ct);
                        }
                    }

                    _consumer.Commit(cr);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                 
                }
            }
        }, ct);

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _consumer.Close();
            _consumer.Dispose();
            return base.StopAsync(cancellationToken);
        }
    }
}