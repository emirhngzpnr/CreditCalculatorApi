using Confluent.Kafka;
using CreditCalculatorApi.Events;
using CreditCalculatorApi.Messaging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using CreditCalculatorApi.Helpers;
using System.Text.Json.Serialization;
using CreditCalculatorApi.Messaging.Interfaces;

namespace CreditCalculatorApi.BackgroundServices
{
    public class RiskEvaluationConsumer : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IKafkaProducer _producer;
        private readonly KafkaOptions _opt;
        private readonly ILogger<RiskEvaluationConsumer> _log;

        
        private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
             Converters = { new JsonStringEnumConverter() }
        };

        public RiskEvaluationConsumer(
            IOptions<KafkaOptions> options,
            IKafkaProducer producer,
            ILogger<RiskEvaluationConsumer> log)
        {
            _opt = options.Value;
            _producer = producer;
            _log = log;

            var cfg = new ConsumerConfig
            {
                BootstrapServers = _opt.BootstrapServers,
                GroupId = "risk-evaluator",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };
            _consumer = new ConsumerBuilder<string, string>(cfg).Build();
        }

        protected override Task ExecuteAsync(CancellationToken ct)
        {
            _consumer.Subscribe(_opt.Topics.CreditApplicationCreated);

            return Task.Run(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    ConsumeResult<string, string>? cr = null;
                    try
                    {
                        cr = _consumer.Consume(ct);

                        
                        _log.LogInformation("creditapp.created raw: {Payload}", cr.Message.Value);

                     
                        var created = JsonSerializer.Deserialize<CreditApplicationCreated>(cr.Message.Value, JsonOpts);
                        if (created is null)
                        {
                            _log.LogWarning("CreditApplicationCreated deserialize null geldi.");
                            _consumer.Commit(cr); 
                            continue;
                        }

                       
                        if (created.ApplicationId <= 0)
                        {
                            _log.LogWarning("ApplicationId <= 0 geldi, mesaj ilerletiliyor. Payload: {Payload}", cr.Message.Value);
                            _consumer.Commit(cr);
                            continue;
                        }

                       
                        var installment = created.CreditTerm > 0
                            ? decimal.Round(created.CreditAmount / created.CreditTerm, 2)
                            : 0m;

                        var dti = (created.MonthlyIncome > 0)
                            ? decimal.Round(installment / created.MonthlyIncome, 4)
                            : 1m; 

                       
                        var label = RiskCalculator.Calculate(created.CreditAmount, created.CreditTerm, created.MonthlyIncome);
                     
                        var risk = label switch
                        {
                            var s when s.Equals("safe", StringComparison.OrdinalIgnoreCase) => "Safe",
                            var s when s.Equals("medium", StringComparison.OrdinalIgnoreCase) => "Medium",
                            var s when s.Equals("risky", StringComparison.OrdinalIgnoreCase) => "Risky",
                            _ => label
                        };

                       
                        var evt = new RiskEvaluatedEvent
                        {
                            ApplicationId = created.ApplicationId,
                            UserNumber = created.UserNumber,    
                            MonthlyIncome = created.MonthlyIncome,
                            MonthlyInstallment = installment,
                            Dti = dti,
                            RiskLabel = risk,
                            BankId = 0,       
                            CampaignId = created.CampaignId,
                            Amount = created.CreditAmount,
                            Term = created.CreditTerm
                        };
                        //!!
                        await _producer.ProduceAsync(_opt.Topics.RiskEvaluated, evt);

                      
                        _consumer.Commit(cr);

                        _log.LogInformation("RiskEvaluated → AppId={AppId} DTI={DTI} Risk={Risk}",
                            evt.ApplicationId, evt.Dti, evt.RiskLabel);
                    }
                    catch (OperationCanceledException) { }
                    catch (JsonException jx)
                    {
                        _log.LogError(jx, "RiskEvaluationConsumer JSON hatası. Mesaj atlanıyor.");
                        if (cr != null) _consumer.Commit(cr);
                    }
                    catch (Exception ex)
                    {
                        _log.LogError(ex, "RiskEvaluationConsumer error (yeniden denenecek).");
                      
                    }
                }
            }, ct);
        }
    }
}
