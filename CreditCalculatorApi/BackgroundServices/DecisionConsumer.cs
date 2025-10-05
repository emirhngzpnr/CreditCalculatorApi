using Confluent.Kafka;
using CreditCalculatorApi.Data;
using CreditCalculatorApi.Entities;
using CreditCalculatorApi.Entities.Enums;
using CreditCalculatorApi.Events;
using CreditCalculatorApi.Messaging;
using CreditCalculatorApi.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using CreditCalculatorApi.Monitoring;
using System.Diagnostics;

namespace CreditCalculatorApi.BackgroundServices
{
    public class DecisionConsumer : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly KafkaOptions _opt;
        private readonly ILogger<DecisionConsumer> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public DecisionConsumer(
            IOptions<KafkaOptions> options,
            IServiceScopeFactory scopeFactory,
            ILogger<DecisionConsumer> logger)
        {
            _opt = options.Value;
            _scopeFactory = scopeFactory;
            _logger = logger;

            var cfg = new ConsumerConfig
            {
                BootstrapServers = _opt.BootstrapServers,
                GroupId = "decision-consumer",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };
            _consumer = new ConsumerBuilder<string, string>(cfg).Build();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(_opt.Topics.RiskEvaluated);

            return Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    ConsumeResult<string, string>? cr = null;
                    try
                    {
                        cr = _consumer.Consume(stoppingToken);

                    
                        _logger.LogInformation("risk.evaluated raw: {Payload}", cr.Message.Value);

                      
                        var evt = JsonSerializer.Deserialize<RiskEvaluatedEvent>(cr.Message.Value, JsonOpts);
                        if (evt is null)
                        {
                            _logger.LogWarning("RiskEvaluatedEvent deserialize null geldi.");
                            _consumer.Commit(cr); 
                            continue;
                        }

                        using var scope = _scopeFactory.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var policy = scope.ServiceProvider.GetRequiredService<IPolicyEngine>();

                        var (outcome, ruleId, version, reason) =
                            await policy.DecideAsync(evt.Dti, evt.RiskLabel, evt.Amount, evt.Term, evt.CampaignId, evt.BankId);

                   
                        var decision = new CreditDecision
                        {
                            Id = Guid.NewGuid(),
                            ApplicationId = evt.ApplicationId,
                            Decision = outcome,
                            RuleId = ruleId,
                            Reason = reason,
                            Version = version,
                            InputsJson = JsonSerializer.Serialize(new
                            { evt.Dti, evt.RiskLabel, evt.Amount, evt.Term, evt.CampaignId, evt.BankId })
                        };
                        db.Add(decision);

                   
                        var app = await db.CreditApplications.FindAsync(evt.ApplicationId);
                        if (app != null)
                        {
                            app.Status = outcome switch
                            {
                                DecisionOutcome.Approved => CreditApplicationStatus.Approved,
                                DecisionOutcome.Declined => CreditApplicationStatus.Rejected,
                                _ => CreditApplicationStatus.Waiting
                            };
                        }

                        switch (outcome)
                        {
                            case DecisionOutcome.Approved:
                                AppMetrics.CreditAppsApprovedTotal
                                    .WithLabels(app?.BankName ?? "Unknown")
                                    .Inc();
                                break;

                            case DecisionOutcome.Declined:
                                AppMetrics.CreditAppsRejectedTotal
                                    .WithLabels(app?.BankName ?? "Unknown")
                                    .Inc();
                                break;
                        }

                        var decisionEvent = new DecisionMadeEvent
                        {
                            ApplicationId = evt.ApplicationId,
                            Decision = outcome,
                            RuleId = ruleId,
                            Reason = reason,
                            Inputs = new { evt.Dti, evt.RiskLabel, evt.Amount, evt.Term, evt.CampaignId, evt.BankId },
                            Version = version
                        };
                        db.Set<OutboxMessage>().Add(new OutboxMessage
                        {
                            Type = typeof(DecisionMadeEvent).AssemblyQualifiedName!,
                            Payload = JsonSerializer.Serialize(decisionEvent),
                            AggregateId = evt.ApplicationId.ToString()
                        });

                        await db.SaveChangesAsync(stoppingToken); 

                    
                        _consumer.Commit(cr);

                        _logger.LogInformation("Decision stored & outboxed | AppId={AppId} Outcome={Outcome}",
                            evt.ApplicationId, outcome);
                    }
                    catch (OperationCanceledException) { /* normal stop */ }
                    catch (JsonException jx)
                    {
                        _logger.LogError(jx, "DecisionConsumer JSON parse hatası. Mesaj ilerletiliyor.");
                        if (cr != null) _consumer.Commit(cr); 
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "DecisionConsumer error (işlenemedi). DLQ önerilir.");
                      
                    }
                }
            }, stoppingToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _consumer.Close();
            _consumer.Dispose();
            return base.StopAsync(cancellationToken);
        }
    }
}
