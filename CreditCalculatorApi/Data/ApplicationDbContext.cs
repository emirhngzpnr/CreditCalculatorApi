using System.Collections.Generic;
using CreditCalculatorApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace CreditCalculatorApi.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<LogModel> AppLogs { get; set; }

        // kredi başvurusu esnasında istediğimiz bilgileri tutan tablo
        public DbSet<CreditApplication> CreditApplications { get; set; }

        // banka bilgilerini tutan tablo
        public DbSet<Bank> Banks { get; set; }
        // kredi hesaplama bilgilerini tutan tablo
        public DbSet<CreditCalculation> CreditCalculations { get; set; }
        // banka müşterisi olma bilgilerini tutan tablo
        public DbSet<CustomerApplication> CustomerApplications { get; set; }

        // kampanya bilgilerini tutan tablo
        public DbSet<Campaign> Campaigns { get; set; }

        // kullanıcı kayıtlarını tutan tablo
        public DbSet<User> Users { get; set; }
        public DbSet<CreditDecision> CreditDecisions {get;set;}
        public DbSet<DecisionPolicy> DecisionPolicies { get; set; }
        public DbSet<OutboxMessage> OutboxMessage { get; set; }
        public DbSet<DecisionNotification> DecisionNotifications { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

         
            modelBuilder.Entity<CustomerApplication>()
                .HasIndex(x => x.CustomerNumber)
                .IsUnique();
            modelBuilder.Entity<CreditDecision>()
    .Property(x => x.Decision)
    .HasConversion<string>()           
    .HasMaxLength(32);
            modelBuilder.Entity<DecisionNotification>()
    .HasIndex(x => new { x.ApplicationId, x.Decision })
    .IsUnique();

        }

    }
}

