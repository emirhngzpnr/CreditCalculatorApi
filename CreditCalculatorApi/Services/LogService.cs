using CreditCalculatorApi.Data;
using CreditCalculatorApi.Entities;
using CreditCalculatorApi.Events;
using CreditCalculatorApi.Messaging;
using CreditCalculatorApi.Messaging.Interfaces;
using CreditCalculatorApi.Services.Interfaces;
using Microsoft.Extensions.Options;
using CreditCalculatorApi.Monitoring;  


namespace CreditCalculatorApi.Services
{
    public class LogService:ILogService
    {
        
        private readonly IKafkaProducer _producer;

        private readonly KafkaOptions _opt;
        private readonly IHttpContextAccessor _http;

        public LogService(IKafkaProducer producer, IHttpContextAccessor httpContextAccessor,IOptions<KafkaOptions> opt)
        {
            _producer = producer;
            _http = httpContextAccessor;
            _opt = opt.Value;
        }

      public async Task LogAsync(LogModel model)
        
        {
           
            var userId = string.IsNullOrEmpty(model.UserId)
                ? _http.HttpContext?.User.FindFirst("userId")?.Value
                : model.UserId;

            var createdAt = model.CreatedAt == default
                ? DateTime.UtcNow
                : model.CreatedAt;

            var correlationId = _http.HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
          /*  try
            {
                var service = (model.Source ?? "unknown").Trim().ToLowerInvariant();
                var opLabel = "general"; 
                var kind = (model.LogType ?? "info").Trim().ToLowerInvariant();

                switch (kind)
                {
                    case "error":
                        AppMetrics.AppErrorTotal.WithLabels(service, opLabel).Inc();
                        break;
                    case "warning":
                    case "warn":
                        AppMetrics.AppWarningTotal.WithLabels(service, opLabel).Inc();
                        break;
                    default:
                        AppMetrics.AppInfoTotal.WithLabels(service, opLabel).Inc();
                        break;
                }
            }
            catch {  }*/

            var evt = new AppLogEvent
            {
                LogType = model.LogType ?? "Info",
                Message = model.Message ?? "",
                UserId = userId,
                Source = model.Source,
                CreatedAtUtc = createdAt,
                Data = model.Data,
                Exception = model.Exception,
                CorrelationId = correlationId
            };

            // !!
            await _producer.ProduceAsync(_opt.Topics.AppLogs, evt, correlationId);
        }
    }
}

