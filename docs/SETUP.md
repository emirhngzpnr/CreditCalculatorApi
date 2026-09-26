# Local setup

[Back to README](../README.md) · [Türkçe özet](../README.tr.md)

This guide describes the configuration expected by the checked-in code. The application and infrastructure run separately. The repository is an internship snapshot with local machine assumptions, so review the paths and configuration below before starting it.

## Prerequisites

- Windows for the current native PDF loading path: `Program.cs` unconditionally loads `DinkToPdf/libwkhtmltox.dll`.
- .NET 8 SDK.
- Node.js compatible with Angular 20. The checked-in Angular packages require Node `^20.19.0 || ^22.12.0 || >=24.0.0`; npm is used for the frontend.
- SQL Server accessible from the host.
- Docker with Compose for Kafka, ZooKeeper, MongoDB, and Redis.
- An SMTP account or TLS-capable test SMTP service for email-related flows.

Other operating systems require adapting the native PDF library loading; the current repository does not provide a cross-platform startup path.

## 1. Clone the repository

```bash
git clone https://github.com/emirhngzpnr/CreditCalculatorApi.git
cd CreditCalculatorApi
```

The commands below use PowerShell unless otherwise noted.

## 2. Prepare infrastructure

In [the infrastructure Compose file](../CreditCalculatorApi/docker/docker-compose.yml), replace both machine-specific JMX volume sources with the path to your checkout's `monitoring/jmx` directory. A relative source from this Compose directory is `../../monitoring/jmx:/jmx:ro`.

Create the shared Docker network once, then start the infrastructure:

```powershell
docker network create obs-net
$env:REDIS_PASSWORD = "<your-local-redis-password>"
docker compose -f CreditCalculatorApi/docker/docker-compose.yml up -d
```

If `obs-net` already exists, reuse it. Replace the Redis placeholder before running the command and use the same value in the API configuration.

| Service | Host port |
| --- | --- |
| Kafka | 9092 |
| ZooKeeper | 2181 |
| MongoDB | 27017 |
| Redis | 6379 |
| Kafka UI | 8080 |
| RedisInsight | 5540 |

SQL Server is **not** included in this Compose file; configure it separately. The Compose file uses the image tags recorded during development. If a tag cannot be pulled, resolve a compatible image/version before continuing.

## 3. Configure the API

Create `CreditCalculatorApi/appsettings.Development.json` inside the backend directory. This file is ignored by Git. There is no checked-in `appsettings.example.json`; use this template:

Generate a 32-byte encryption key and keep the resulting Base64 value outside source control:

```powershell
$keyBytes = New-Object byte[] 32
[Security.Cryptography.RandomNumberGenerator]::Fill($keyBytes)
[Convert]::ToBase64String($keyBytes)
```

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CreditCalculatorLocal;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "<replace-with-a-random-secret-of-at-least-32-bytes>",
    "Issuer": "CreditCalculatorApi",
    "Audience": "CreditCalculatorFrontend"
  },
  "Encryption": {
    "MasterKey": "<replace-with-the-generated-32-byte-base64-key>"
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092",
    "ClientId": "creditcalc-api",
    "Topics": {
      "CreditApplicationCreated": "creditapp.created",
      "CreditApplicationStatusChanged": "creditapp.status.changed",
      "RiskEvaluated": "risk.evaluated",
      "DecisionMade": "decision.made",
      "AppLogs": "app.logs"
    }
  },
  "Mongo": {
    "ConnectionString": "mongodb://localhost:27017",
    "Database": "CreditCalculatorRead",
    "Collections": {
      "Events": "DomainEvents",
      "CreditApps": "CreditApplicationsRead",
      "Logs": "Logs"
    }
  },
  "Redis": {
    "Password": "<your-local-redis-password>",
    "InstanceName": "ccapi_"
  },
  "Email": {
    "SmtpServer": "<smtp-host>",
    "Port": 587,
    "Username": "<smtp-username>",
    "Password": "<smtp-password>",
    "From": "<sender-email>"
  }
}
```

Replace all placeholders. The SQL example uses Windows authentication; change it if your SQL Server uses another authentication method. JWT issuer and audience must remain consistent with the API's token configuration. Keep the encryption key stable for an existing database: changing it prevents the API from decrypting identity numbers written with the previous key. Migrate or recreate development data when rotating this key.

These keys come from `Program.cs`, `KafkaOptions`, `MongoOptions`, `JwtService`, and `EmailService`. Environment variables such as `ConnectionStrings__DefaultConnection` can override JSON values. The EF design-time factory reads the JSON files and environment variables directly.

## 4. Apply migrations and start the API

Run from the backend directory so the native PDF library path resolves correctly:

```powershell
cd CreditCalculatorApi
dotnet restore
dotnet build
```

If `dotnet-ef` is not installed, install the version matching the EF Core packages:

```powershell
dotnet tool install --global dotnet-ef --version 9.0.7
```

Then apply migrations to your local database and start the HTTPS profile:

```powershell
dotnet ef database update
dotnet dev-certs https --trust
dotnet run --launch-profile https
```

Open [Swagger](https://localhost:7152/swagger). The profile sets `ASPNETCORE_ENVIRONMENT=Development`, which enables Swagger and loads the development settings.

MongoDB must be available at startup because the API creates a log index before building the application. SQL Server and Kafka also need to be ready for the background consumers and startup queries.

## 5. Start the frontend

Open another terminal at the repository root:

```powershell
cd Frontend
npm ci
npm start
```

Open [the Angular application](http://localhost:4200). Frontend services reference `https://localhost:7152/api`, and the API CORS policy permits `http://localhost:4200`. Keep these addresses aligned if you change ports.

Use synthetic accounts and application data. Registration and notification flows require working email configuration. The repository does not supply a documented pre-seeded administrator account.

## 6. Optional monitoring

Before starting [the monitoring Compose file](../monitoring/docker-compose.yml), copy `monitoring/.env.example` to `monitoring/.env`, fill in the Grafana admin password and SMTP credentials, and review the Grafana data bind mount. `monitoring/.env` is ignored by Git. Use a fresh local Grafana data directory for a clean setup.

From the repository root:

```powershell
docker compose -f monitoring/docker-compose.yml up -d
```

- [Prometheus](http://localhost:9090) scrapes the API and configured exporters.
- [Grafana](http://localhost:3000) loads the provisioned datasource and dashboards.
- API metrics are exposed at [`/metrics`](https://localhost:7152/metrics).

The Prometheus configuration targets `host.docker.internal:7152` for the host API and includes a development TLS setting. The host metrics exporters have Docker/Linux-specific mounts; their availability depends on the Docker environment.

Dashboard JSON files are under `monitoring/grafana/dashboards`. Alert definitions and runbooks are under `monitoring/grafana/alerts`; the Compose file does not mount that directory as Grafana alert provisioning, so those definitions are not automatically activated by the startup command.

## Verification commands

```powershell
# From CreditCalculatorApi/ (the backend directory)
dotnet build

# From Frontend/
npm run build
npm test
```

The frontend includes Jasmine/Karma spec files. The repository does not currently contain a separate backend test project. These commands are provided for local verification; the documentation update does not establish that the full stack has passed an end-to-end run.

## Common setup issues

| Symptom | Check |
| --- | --- |
| API fails before Swagger opens | MongoDB availability, JWT settings, SQL Server connection, and the native PDF DLL path. |
| PDF library cannot be loaded | Run from the backend directory on Windows and check the native library/process architecture. |
| API calls fail from Angular | API HTTPS profile, trusted local certificate, frontend API URLs, and the CORS origin. |
| Applications remain pending | Kafka connectivity, consumer logs, migrations, and the risk/decision event flow. |
| Emails fail | SMTP host, port, credentials, sender address, and TLS support. |
| Infrastructure cannot start | Shared `obs-net` network, JMX volume paths, Redis password, and image availability. |
