# Kredi Hesaplama ve Başvuru Yönetimi

**ASP.NET Core 8 ve Angular 20 ile geliştirdiğim full-stack staj projem.**

Kredi ödeme planı hesaplama, müşteri ve kredi başvuruları, asenkron risk değerlendirme ve uygulama izleme bileşenlerini bir araya getirir.

[English](README.md) · [Yerel kurulum rehberi (İngilizce)](docs/SETUP.md)

## Projenin hikâyesi

Bu projeyi **Temmuz–Eylül 2025 döneminde VakıfBank'ta yaptığım yazılım mühendisliği stajı sırasında** geliştirdim. İlk ASP.NET Core projem olarak, öğrendiğim kavramları bankacılıkla ilişkili bir iş akışı üzerinde uygulamamı sağladı. API geliştirme ve veri erişimiyle başlayan çalışma; Angular arayüzü, arka plan işlemleri ve izleme bileşenleriyle genişledi.

Proje, departmanımdaki mühendislerden ve staj sorumlularımdan olumlu geri bildirim aldı. Bu depo, staj boyunca geliştirdiğim uygulamayı ve edindiğim teknik deneyimi paylaşmak için hazırlanmıştır.

**Kapsam:** Eğitim ve portföy projesidir. Hesaplama ve karar kuralları örnek uygulama akışlarını gösterir; VakıfBank'ın kredi politikalarını veya üretimde kullanılan bir bankacılık hizmetini temsil etmez.

## Neler yapabiliyor?

| Alan | Özellikler |
| --- | --- |
| Kredi hesaplama | Aylık taksit, toplam geri ödeme ve itfa tablosu oluşturma; hesaplamaları kaydetme ve raporlama. |
| Kullanıcı işlemleri | Kayıt, JWT ile giriş, e-posta doğrulama, parola sıfırlama ve profil yönetimi. |
| Banka ve kampanyalar | Bankaları görüntüleme, kampanyaları banka ve kredi türüne göre filtreleme; yönetici arayüzünde banka ve kampanya yönetimi. |
| Başvurular | Müşteri ve kredi başvurusu oluşturma, başvuru geçmişini ve onaylanan kredileri görüntüleme, başvuru durumlarını güncelleme. |
| Risk ve karar akışı | Kafka üzerinden başvuru olaylarını işleme, basitleştirilmiş risk sınıflandırması ve onay/ret/manuel inceleme kararı. |
| Bildirim ve belgeler | Başvuru ve durum e-postaları, DinkToPdf ile PDF üretimi ve arayüzde jsPDF tabanlı dışa aktarma. |
| Loglama ve izleme | Kafka üzerinden SQL Server ve MongoDB'ye log aktarımı; Prometheus metrikleri, Grafana panoları ve alarm tanımları. |

## Teknolojiler

| Katman | Kullanılan teknolojiler |
| --- | --- |
| Backend | C#, ASP.NET Core 8, Entity Framework Core 9, FluentValidation, Swagger |
| Frontend | Angular 20, TypeScript, Bootstrap 5, RxJS |
| Veri ve önbellek | SQL Server, MongoDB, Redis |
| Mesajlaşma | Apache Kafka, Confluent.Kafka |
| Kimlik doğrulama | JWT, BCrypt parola hashleme, AES şifreleme uygulaması |
| İzleme | Serilog, prometheus-net, Prometheus, Grafana, JMX exporter'ları |
| Belgeler ve yerel altyapı | DinkToPdf / wkhtmltox, jsPDF, Docker Compose |

## Başvuru akışı

Backend; controller, service, repository ve arka plan tüketicileri olarak düzenlenmiş tek bir ASP.NET Core uygulamasıdır. Kafka tüketicileri bu uygulama içinde hosted service olarak çalışır.

1. Kredi başvurusu SQL Server'a kaydedilir ve `creditapp.created` olayı yayımlanır.
2. Risk tüketicisi basitleştirilmiş taksit/gelir oranını ve risk etiketini hesaplayıp `risk.evaluated` olayını yayımlar.
3. Karar tüketicisi politikayı uygular; kararı, başvuru durumunu ve outbox kaydını aynı veritabanı kayıt işleminde saklar.
4. Outbox yayıncısı bekleyen karar olaylarını `decision.made` konusuna gönderir.
5. Bildirim tüketicisi durum e-postasını gönderir ve bildirim kaydını saklar.

Mevcut politika **Safe → Onay**, **Risky → Ret**, **Medium / diğer → Manuel inceleme** eşlemesini kullanır. Risk hesabındaki tutar/vade yaklaşımı, faiz içeren kredi ödeme planı hesabından ayrıdır.

Mimari diyagram ve ilgili kaynak kod bağlantıları için [İngilizce README](README.md#architecture) bölümüne bakabilirsiniz.

## Yerel çalıştırma

[Kurulum rehberi](docs/SETUP.md), gerekli servisleri, ayar anahtarlarını, veritabanı migration adımlarını ve çalıştırma komutlarını açıklar.

- Arayüz: `http://localhost:4200`
- Swagger: `https://localhost:7152/swagger`
- Metrikler: `https://localhost:7152/metrics`
- İzleme servisleri başlatıldığında Grafana: `http://localhost:3000`

Mevcut kurulum Windows'a özgü bir PDF kütüphanesi yükler ve Docker dosyalarında bilgisayara özel yollar içerir; çalıştırmadan önce rehberdeki ayarları uyarlamak gerekir.

### Örnek kredi hesaplama isteği

```http
POST /api/credits/hesapla-ve-kaydet
Content-Type: application/json

{
  "krediTutari": 100000,
  "vade": 12,
  "faizOrani": 3
}
```

`vade` ay sayısını, `faizOrani` ise **aylık yüzde faiz oranını** belirtir. Örnekteki `3`, aylık %3 anlamına gelir. Yanıt; aylık taksiti, toplam geri ödemeyi ve taksitlerin faiz/anapara dağılımını içerir. Sonuç ayrıca veritabanına kaydedilir. Hesaplama, bankaya özgü masraf, vergi veya sigorta kalemlerini eklemez.

## Bu projeyle kazandığım deneyim

- Dependency injection, DTO, doğrulama ve migration kullanarak ASP.NET Core API geliştirmek.
- Angular arayüzünü kimlik doğrulamalı API'lerle birleştirmek ve kullanıcı/yönetici ekranlarını ayırmak.
- Bir iş sürecini Kafka olayları, tüketiciler, veritabanı güncellemeleri ve outbox üzerinden takip etmek.
- SQL Server ve MongoDB ile çalışmak, Redis önbellek yapılandırmasını uygulamaya eklemek.
- Log, metrik, pano ve alarm tanımlarıyla uygulama davranışını gözlemlemek.

## Projenin durumu

Depo, öğrenme sürecimin erken bir dönemindeki çalışmamı korur. Otomatik entegrasyon testleri, taşınabilir yapılandırma, gizli anahtar yönetimi, loglarda hassas veri maskeleme ve olay akışında daha kapsamlı tekrar deneme/idempotency davranışı sonraki geliştirme alanlarıdır. Yerel denemelerde sentetik veriler kullanılmalıdır.

## Geliştirici

**Emirhan Efe Gözpınar**  
VakıfBank Yazılım Mühendisliği Stajyeri · Temmuz–Eylül 2025  
[GitHub profili](https://github.com/emirhngzpnr)

