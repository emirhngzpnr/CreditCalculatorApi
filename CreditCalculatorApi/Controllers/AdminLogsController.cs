using CreditCalculatorApi.DTOs.Logs;
using CreditCalculatorApi.Events;
using CreditCalculatorApi.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;


namespace CreditCalculatorApi.Controllers
{

    [ApiController]
    [Route("api/admin/logs")]
    [Authorize(Roles = "Admin")]
    [Produces("application/json")]

    public class AdminLogsController:ControllerBase
    {
        private readonly MongoReadDb _mongo;
        private readonly ILogger<AdminLogsController> _logger;

        public AdminLogsController(MongoReadDb mongo, ILogger<AdminLogsController> logger)
        {
            _mongo = mongo;
            _logger = logger;
        }
        /// <summary>
        /// Admin paneli için logları filtreleyip sayfalı şekilde listeler.
        /// </summary>
        /// <param name="q">Filtre ve sayfalama parametreleri</param>
        /// <returns>Filtrelenmiş ve sayfalı log listesi</returns>
        /// <response code="200">Loglar başarıyla listelendi</response>
        /// <response code="400">Geçersiz filtre parametreleri</response>
        /// <response code="401">Kullanıcı doğrulanamadı</response>
        /// <response code="403">Yetkisiz erişim</response>

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<LogItemDto>>> Get([FromQuery] LogsQueryDto q, CancellationToken ct)
        {
            // Guard: sayfalama sınırları
            if (q.Page <= 0) q.Page = 1;
            if (q.PageSize < 1) q.PageSize = 20;
            if (q.PageSize > 200) q.PageSize = 200;

            // Filter compose
            var filter = Builders<AppLogEvent>.Filter.Empty;

            if (!string.IsNullOrWhiteSpace(q.Level) && q.Level.ToLower() != "all")
                filter &= Builders<AppLogEvent>.Filter.Eq(x => x.LogType, q.Level);


            if (!string.IsNullOrWhiteSpace(q.Search))
            {
                // basit case-insensitive regex arama: message / source / exception
                var regex = new BsonRegularExpression(q.Search, "i");
                filter &= Builders<AppLogEvent>.Filter.Or(
                    Builders<AppLogEvent>.Filter.Regex(x => x.Message!, regex),
                    Builders<AppLogEvent>.Filter.Regex(x => x.Source!, regex),
                    Builders<AppLogEvent>.Filter.Regex(x => x.Exception!, regex)
                );
            }

            if (q.FromUtc.HasValue)
                filter &= Builders<AppLogEvent>.Filter.Gte(x => x.CreatedAtUtc, q.FromUtc.Value);

            if (q.ToUtc.HasValue)
                filter &= Builders<AppLogEvent>.Filter.Lte(x => x.CreatedAtUtc, q.ToUtc.Value);

            var sort = Builders<AppLogEvent>.Sort.Descending(x => x.CreatedAtUtc);

            // total
            var total = await _mongo.Logs.CountDocumentsAsync(filter, cancellationToken: ct);

            // page
            var items = await _mongo.Logs
                .Find(filter)
                .Sort(sort)
                .Skip((q.Page - 1) * q.PageSize)
                .Limit(q.PageSize)
                .Project(x => new LogItemDto
                {
                    LogType = x.LogType,
                    Message = x.Message,
                    Source = x.Source,
                    UserId = x.UserId,
                    CreatedAtUtc = x.CreatedAtUtc,
                    CorrelationId = x.CorrelationId,
                    Exception = x.Exception,
                    Data = x.Data
                })
                .ToListAsync(ct);

            return Ok(new PagedResultDto<LogItemDto>(items, total, q.Page, q.PageSize));
        }

        /// <summary>
        /// Tek bir log kaydının detayını döner.
        /// </summary>
        /// <param name="id">Mongo ObjectId</param>
        /// <returns>Log detay bilgisi</returns>
        /// <response code="200">Log bulundu</response>
        /// <response code="404">Log bulunamadı</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, CancellationToken ct)
        {
            if (!ObjectId.TryParse(id, out var oid))
                return NotFound(new ProblemDetails { Title = "Geçersiz Id", Detail = "ObjectId formatı bekleniyor." });

            var filter = Builders<AppLogEvent>.Filter.Eq("_id", oid);
            var x = await _mongo.Logs.Find(filter).FirstOrDefaultAsync(ct);
            if (x == null) return NotFound();

            var dto = new LogItemDto
            {
                LogType = x.LogType,
                Message = x.Message,
                Source = x.Source,
                UserId = x.UserId,
                CreatedAtUtc = x.CreatedAtUtc,
                CorrelationId = x.CorrelationId,
                Exception = x.Exception,
                Data = x.Data
            };
            return Ok(dto);
        }
    }
}
