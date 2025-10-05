using Prometheus;

namespace CreditCalculatorApi.Monitoring;

public static class AppMetrics
{
  
    //kredi işlemleri için
    public static readonly Counter CreditAppsCreatedTotal =
        Metrics.CreateCounter(
            "credit_applications_created_total",
            "Toplam oluşturulan başvuru",
            new CounterConfiguration { LabelNames = new[] { "bank", "credit_type" } });

    

    public static readonly Counter CreditAppsApprovedTotal =
        Metrics.CreateCounter(
            "credit_applications_approved_total",
            "Onaylanan başvuru",
            new CounterConfiguration { LabelNames = new[] { "bank" } });

   
    public static readonly Histogram CreditDecisionLatencySeconds =
        Metrics.CreateHistogram(
            "credit_decision_latency_seconds",
            "Karar süresi (saniye)",
            new HistogramConfiguration
            {
                Buckets = Histogram.ExponentialBuckets(start: 0.05, factor: 1.8, count: 10),
                LabelNames = new[] { "policy" }
            });

   
    public static readonly Gauge OutboxPendingGauge =
        Metrics.CreateGauge(
            "outbox_pending_messages",
            "Outbox kuyruğunda bekleyen mesaj sayısı");
    public static readonly Counter CreditAppsRejectedTotal =
    Metrics.CreateCounter(
        "credit_applications_rejected_total",
        "Reddedilen kredi başvurusu sayısı",
        new CounterConfiguration
        {
            LabelNames = new[] { "bank" }
        });
    // kullanıcı işlemleri için
    public static readonly Counter UserRegisteredTotal =
      Metrics.CreateCounter(
          "user_registered_total",
          "Toplam kullanıcı kayıt sayısı (event bazlı).",
          new CounterConfiguration { LabelNames = new[] { "source" } }
      );
    public static readonly Counter UserLoginTotal =
     Metrics.CreateCounter(
         "user_login_total",
         "Toplam kullanıcı giriş sayısı (event bazlı, successful).",
         new CounterConfiguration { LabelNames = new[] { "method" } }
     );
    // banka işlemleri için
    public static readonly Counter CustomerAppsCreatedTotal =
    Metrics.CreateCounter(
        "customer_applications_created_total",
        "Toplam müşteri olma (bankaya üye olma) başvuruları",
        new CounterConfiguration { LabelNames = new[] { "bank" } });

    public static readonly Counter CustomerAppsApprovedTotal =
    Metrics.CreateCounter(
        "customer_applications_approved_total",
        "Onaylanan müşteri olma başvuruları",
        new CounterConfiguration { LabelNames = new[] { "bank" } });

    public static readonly Counter CustomerAppsRejectedTotal =
        Metrics.CreateCounter(
            "customer_applications_rejected_total",
            "Reddedilen müşteri olma başvuruları",
            new CounterConfiguration { LabelNames = new[] { "bank" } });

    // error ve log işlemleri için
    public static readonly Counter AppInfoTotal =
      Metrics.CreateCounter(
          "app_info_total",
          "Toplam bilgi log sayısı",
          new CounterConfiguration { LabelNames = new[] { "service", "operation" } });

    public static readonly Counter AppWarningTotal =
        Metrics.CreateCounter(
            "app_warning_total",
            "Toplam uyarı log sayısı",
            new CounterConfiguration { LabelNames = new[] { "service", "operation" } });

    public static readonly Counter AppErrorTotal =
        Metrics.CreateCounter(
            "app_error_total",
            "Toplam hata log sayısı",
            new CounterConfiguration { LabelNames = new[] { "service", "operation" } });


}
