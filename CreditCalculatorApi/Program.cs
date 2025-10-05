using System.Reflection;
using CreditCalculatorApi.Services;
using Microsoft.EntityFrameworkCore;
using CreditCalculatorApi.Data;
using CreditCalculatorApi.Services.Interfaces;
using CreditCalculatorApi.Repository.Interfaces;
using CreditCalculatorApi.Repository;
using Serilog;
using CreditCalculatorApi.Validators;
using FluentValidation.AspNetCore;
using FluentValidation;
using CreditCalculatorApi.DTOs;
using System.Text.Json.Serialization;
using CreditCalculatorApi.Security.Interfaces;
using CreditCalculatorApi.Security;
using CreditCalculatorApi.Services.Auth;
using CreditCalculatorApi.Services.Account;
using CreditCalculatorApi.Services.Profile;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DinkToPdf.Contracts;
using DinkToPdf;
using CreditCalculatorApi.Helpers;
using CreditCalculatorApi.Services.Notification;

using CreditCalculatorApi.Messaging;
using CreditCalculatorApi.ReadModels;
using CreditCalculatorApi.BackgroundServices;
using CreditCalculatorApi.Services.Decision.Policy;
using Confluent.Kafka;
using CreditCalculatorApi.Messaging.Interfaces;
using CreditCalculatorApi.Events;
using MongoDB.Driver;
using Prometheus;
using CreditCalculatorApi.Monitoring;
using CreditCalculatorApi.Services.Verification;








var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // appsettings.json'dan oku
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));
builder.Services.Configure<MongoOptions>(builder.Configuration.GetSection("Mongo"));
builder.Services.AddSingleton<MongoReadDb>();
// Program.cs - Build sonrasý bir kere çalýþtýr
var sp = builder.Services.BuildServiceProvider();
using (var scope = sp.CreateScope())
{
    var mongo = scope.ServiceProvider.GetRequiredService<MongoReadDb>();
    var idx = new CreateIndexModel<AppLogEvent>(
        Builders<AppLogEvent>.IndexKeys
            .Ascending(x => x.LogType)
            .Descending(x => x.CreatedAtUtc));
    await mongo.Logs.Indexes.CreateOneAsync(idx);
}

builder.Services.AddHostedService<CreditCalculatorApi.BackgroundServices.CreditAppCreatedConsumer>();
builder.Services.AddScoped<IPolicyEngine, PolicyEngine>();

builder.Services.AddHostedService<RiskEvaluationConsumer>();
builder.Services.AddHostedService<DecisionConsumer>();
builder.Services.AddHostedService<OutboxPublisher>();
builder.Services.AddHostedService<DecisionNotificationConsumer>();

builder.Services.AddHostedService<LogToSqlConsumer>();
builder.Services.AddHostedService<LogToMongoConsumer>();

builder.Services.AddSingleton<IProducer<string, string>>(sp =>
{
    var cfg = new ProducerConfig
    {
        BootstrapServers = builder.Configuration["Kafka:BootstrapServers"],
        ClientId = builder.Configuration["Kafka:ClientId"] ?? "creditcalc-api",
        Acks = Acks.All // Tüm broker'larýn mesajý onaylamasýný bekler
    };
    return new ProducerBuilder<string, string>(cfg).Build();
});

var inContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

var redisHost = inContainer ? "redis" : "localhost";
var kafkaBootstrap = inContainer ? "kafka:29092" : "localhost:9092";
var mongoConn = inContainer ? "mongodb://mongodb:27017" : "mongodb://localhost:27017";
var sqlConn = inContainer
    ? "Server=host.docker.internal,1433;Database=YourDb;User Id=sa;Password=YourPass;TrustServerCertificate=True;"
    : builder.Configuration.GetConnectionString("SqlServer");

// Redis kaydý
builder.Services.AddStackExchangeRedisCache(opt =>
{
    var pass = builder.Configuration["Redis:Password"] ?? "";
    opt.Configuration = $"{redisHost}:6379,password={pass},abortConnect=false";
    opt.InstanceName = builder.Configuration["Redis:InstanceName"] ?? "ccapi_";
});


//serilog kullanýmý için gerekli ayarlar
builder.Host.UseSerilog();

// CORS policy tanýmý
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200") // Angular'ýn çalýþtýðý port
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});



// Add services to the container. IoC IC isterse C ' yi ver bunu tanýtýyoruz.
builder.Services.AddScoped<ICreditService, CreditService>();
builder.Services.AddScoped<IBankService, BankService>();

builder.Services.AddScoped<ICreditApplicationService, CreditApplicationService>();
builder.Services.AddScoped<ICreditCalculationRepository, CreditCalculationRepository>();

builder.Services.AddScoped<ICustomerApplicationService, CustomerApplicationService>();
builder.Services.AddScoped<ICustomerApplicationRepository, CustomerApplicationRepository>();

builder.Services.AddScoped<ICampaignRepository, CampaignRepository>();
builder.Services.AddScoped<ICampaignService, CampaignService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAccountService, AccountService>();

builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<PdfService>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IVerificationService,VerificationService>();

builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();

builder.Services.AddHttpContextAccessor();





// AddDbContext ile MSSQL baðlantýsý tanýmlanýr
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
    // JWT SecurityScheme tanýmý
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header. Örnek: 'Bearer eyJhbGciOiJIUzI1...'"
    });

    // Swagger UI'de global olarak JWT taþýnacaðýný belirt
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

            

// fluentvalidation için gerekli baðýmlýlýklar
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<BankRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CampaignRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreditApplicationRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreditApplicationStatusValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreditRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CustomerApplicationRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ResetPasswordRequestValidator>();

// email gönderimi için gereken baðýmlýlýklar
builder.Services.AddScoped<EmailService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "JwtBearer";
    options.DefaultChallengeScheme = "JwtBearer";
})
.AddJwtBearer("JwtBearer", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});
var context = new CustomAssemblyLoadContext();
context.LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "DinkToPdf", "libwkhtmltox.dll"));

builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

var app = builder.Build();
app.Use(async (ctx, next) =>
{
    if (ctx.Request.Path.StartsWithSegments("/metrics"))
    {
        await next();
        return;
    }

    // diðer tüm yollar için https yönlendirme
    await next();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<CreditCalculatorApi.Middlewares.ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowAngularApp");
app.MapMetrics("/metrics").AllowAnonymous();
app.UseWhen(ctx => !ctx.Request.Path.StartsWithSegments("/metrics"),
    b => b.UseHttpsRedirection());
app.UseHttpMetrics();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var service = scope.ServiceProvider.GetRequiredService<ICampaignService>();
    await service.UpdateExpiredCampaignStatusesAsync();
}
using (var scope = app.Services.CreateScope())
{
    var service = scope.ServiceProvider.GetRequiredService<ICampaignService>();
    await service.UpdateExpiredCampaignStatusesAsync();
}
app.Run();
