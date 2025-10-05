using CreditCalculatorApi.Events;
using CreditCalculatorApi.ReadModels;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CreditCalculatorApi.Messaging
{
  
        public class MongoReadDb
        {
            public IMongoDatabase Db { get; }
            public IMongoCollection<BsonDocument> Events { get; } // log/serbest dökümanlar için
            public IMongoCollection<CreditApplicationCreated> CreditApps { get; } // TIPLI!
        public IMongoCollection<AppLogEvent> Logs { get; }

            public MongoReadDb(IOptions<MongoOptions> opt)
            {
                var client = new MongoClient(opt.Value.ConnectionString);
                Db = client.GetDatabase(opt.Value.Database);
                Events = Db.GetCollection<BsonDocument>(opt.Value.Collections.Events);
                CreditApps = Db.GetCollection<CreditApplicationCreated>(opt.Value.Collections.CreditApps);
            Logs = Db.GetCollection<AppLogEvent>(opt.Value.Collections.Logs);
        }
        }
    }
