using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RS.COMMON.DTO;
using RS.COMMON.Entities;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;
using RS.MVC.Models;
using static RS.COMMON.Constants.Enums;

namespace RS.MVC.Applications
{
    public interface IReviewApplication
    {
        Task<ReviewArticleResult> GetArticleAsync(string articleId);

        Task<ReviewSaveResult> SaveAsync(string articleId, ReviewSaveRequest request, string userName);

        /// <summary>Resistances whose name or company matches, most recent protest first (at most 20).</summary>
        Task<IReadOnlyList<ResistanceOption>> SearchResistancesAsync(string query);
    }

    public enum ReviewStatus
    {
        Ok,
        Invalid,
        AlreadyHandled,
        NotFound,
    }

    public record ReviewArticleResult(ReviewStatus Status, ReviewArticle Article = null);

    public record ReviewSaveResult(
        ReviewStatus Status,
        int? ResistanceId = null,
        bool OutcomeSent = false,
        IReadOnlyDictionary<string, string> Errors = null);

    /// <summary>
    /// The review screen: an article from the inbox becomes part of a case. One transaction saves the thin News row
    /// (Linked, with its ArticleId), the resistance and/or protest, and the link; then Digested is sent to the
    /// pipeline. If that call fails, the inbox re-sends it (same as for dismissals).
    /// </summary>
    public class ReviewApplication : IReviewApplication
    {
        private readonly RSDBContext _db;
        private readonly IResistanceApplication _resistances;
        private readonly IPipelineClient _pipeline;
        private readonly PipelineSettings _settings;
        private readonly ILogger<ReviewApplication> _logger;

        public ReviewApplication(
            RSDBContext db,
            IResistanceApplication resistances,
            IPipelineClient pipeline,
            IOptions<PipelineSettings> settings,
            ILogger<ReviewApplication> logger)
        {
            _db = db;
            _resistances = resistances;
            _pipeline = pipeline;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<ReviewArticleResult> GetArticleAsync(string articleId)
        {
            if (await _db.News.AnyAsync(n => n.ArticleId == articleId))
                return new ReviewArticleResult(ReviewStatus.AlreadyHandled);

            var article = await _pipeline.GetArticleAsync(articleId);
            return article == null
                ? new ReviewArticleResult(ReviewStatus.NotFound)
                : new ReviewArticleResult(ReviewStatus.Ok, new ReviewArticle(
                    article.ArticleId, article.Title, article.Url, ArticleNews.SourceName(article.SourceId), article.PublishedAt, article.Body));
        }

        public async Task<IReadOnlyList<ResistanceOption>> SearchResistancesAsync(string query)
        {
            var text = query?.Trim() ?? "";
            return await _db.Resistance
                .Where(r => !r.Deleted)
                .Where(r => text == "" || r.Name.Contains(text) || (r.Company != null && r.Company.Name.Contains(text)))
                .Select(r => new ResistanceOption(
                    r.Id,
                    r.Name,
                    r.Company != null ? r.Company.Name : null,
                    r.Protestos.Where(p => !p.Deleted).Max(p => (DateTime?)p.StartDate)))
                .OrderByDescending(o => o.LastProtestDate)
                .Take(20)
                .ToListAsync();
        }

        public async Task<ReviewSaveResult> SaveAsync(string articleId, ReviewSaveRequest request, string userName)
        {
            var errors = ReviewValidation.Validate(request);
            if (errors.Count > 0)
                return new ReviewSaveResult(ReviewStatus.Invalid, Errors: errors);

            if (await _db.News.AnyAsync(n => n.ArticleId == articleId))
                return new ReviewSaveResult(ReviewStatus.AlreadyHandled);

            if (request.Action != ReviewAction.NewCase
                && !await _db.Resistance.AnyAsync(r => r.Id == request.ResistanceId && !r.Deleted))
            {
                return new ReviewSaveResult(ReviewStatus.Invalid,
                    Errors: new Dictionary<string, string> { ["resistanceId"] = "Vaka bulunamadı." });
            }

            var article = await _pipeline.GetArticleAsync(articleId);
            if (article == null)
                return new ReviewSaveResult(ReviewStatus.NotFound);

            int resistanceId;
            try
            {
                await using var transaction = await _db.Database.BeginTransactionAsync();

                var news = await ArticleNews.FindOrAddAsync(_db, article, _settings);
                news.Status = Status.Linked;
                await _db.SaveChangesAsync();

                switch (request.Action)
                {
                    case ReviewAction.NewCase:
                        resistanceId = _resistances.Create(ToResistanceModel(request.Case, request.Protest, news, userName));
                        break;
                    case ReviewAction.AddProtest:
                        resistanceId = request.ResistanceId!.Value;
                        _resistances.AddProtesto(ToProtestoAddModel(resistanceId, request.Protest, userName));
                        await LinkAsync(resistanceId, news.Id);
                        break;
                    default:
                        resistanceId = request.ResistanceId!.Value;
                        await LinkAsync(resistanceId, news.Id);
                        break;
                }

                await transaction.CommitAsync();
            }
            catch (DbUpdateException)
            {
                // A colleague saved it a moment earlier (unique ArticleId); anything else is a real error.
                _db.ChangeTracker.Clear();
                if (await _db.News.AnyAsync(n => n.ArticleId == articleId))
                    return new ReviewSaveResult(ReviewStatus.AlreadyHandled);
                throw;
            }

            var sent = true;
            try
            {
                await _pipeline.RecordOutcomeAsync(articleId, new PipelineOutcomeRequest(PipelineOutcomeKind.Digested, resistanceId));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Linked {ArticleId} to resistance {ResistanceId}, but the pipeline could not be told", articleId, resistanceId);
                sent = false;
            }
            return new ReviewSaveResult(ReviewStatus.Ok, resistanceId, sent);
        }

        private async Task LinkAsync(int resistanceId, int newsId)
        {
            if (!await _db.ResistanceNews.AnyAsync(rn => rn.ResistanceId == resistanceId && rn.NewsId == newsId))
            {
                _db.ResistanceNews.Add(new ResistanceNews { ResistanceId = resistanceId, NewsId = newsId });
                await _db.SaveChangesAsync();
            }
        }

        // The existing create logic (ResistanceApplication.Create) takes the old form's model; map the ids onto it.
        private static ResistanceModel ToResistanceModel(CaseInput c, ProtestInput p, News news, string userName)
        {
            var againstCompany = c.TargetType == ResistanceTargetType.Company;
            return new ResistanceModel
            {
                UserName = userName,
                Name = c.Name,
                ResistanceDescription = c.Description,
                CategoryId = c.CategoryId!.Value,
                TargetType = c.TargetType!.Value,
                CompanyId = againstCompany && c.CompanyId is { } companyId ? new Company { Id = companyId } : null,
                MainCompanyId = againstCompany && c.IsOutsource && c.MainCompanyId is { } mainId ? new Company { Id = mainId } : null,
                CorporationId = againstCompany ? null : c.TargetCorporationId,
                ResistanceReasonIds = c.ReasonIds.Select(id => new ResistanceReason { Id = id }).ToList(),
                DevelopRight = c.DevelopRight!.Value,
                EmployeeCount = c.EmployeeCount,
                EmployeeCountId = c.EmployeeCountRangeId,
                CorporationIds = c.CorporationIds.Select(id => new Corporation { Id = id }).ToList(),
                TradeUnionAuthorityId = c.TradeUnionAuthorityId,
                TradeUnionId = c.TradeUnionId,
                EmploymentTypeIds = c.EmploymentTypeIds.Select(id => new EmploymentType { Id = id }).ToList(),
                FiredEmployeeCountByProtesto = c.FiredEmployeeCount ?? 0,
                ResistanceResult = c.Result!.Value,
                ResistanceNews = [news],
                Protesto = new ProtestoModel
                {
                    ProtestoTypeIds = p.ProtestTypeIds.Select(id => new ProtestoType { Id = id }).ToList(),
                    StrikeDuration = p.StrikeDurationSeconds ?? 0,
                    SimpleProtestoDescription = p.SimpleDescription,
                    ProtestoStartDate = p.StartDate!.Value,
                    ProtestoEndDate = p.EndDate,
                    ProtestoPlaceIds = p.PlaceIds.Select(id => new ProtestoPlace { Id = id }).ToList(),
                    GenderId = p.GenderId!.Value,
                    Locations = ToLocations(p),
                    EmployeeCountInProtesto = p.EmployeeCount,
                    EmployeeCountInProtestoId = p.EmployeeCountRangeId,
                    InterventionTypeIds = p.InterventionTypeIds.Select(id => new InterventionType { Id = id }).ToList(),
                    CustodyCount = p.CustodyCount,
                    Note = p.Note,
                },
            };
        }

        private static ProtestoAddModel ToProtestoAddModel(int resistanceId, ProtestInput p, string userName) => new()
        {
            ResistanceId = resistanceId,
            UserName = userName,
            ProtestoTypeIds = p.ProtestTypeIds,
            StrikeDuration = p.StrikeDurationSeconds ?? 0,
            SimpleProtestoDescription = p.SimpleDescription,
            ProtestoStartDate = p.StartDate!.Value,
            ProtestoEndDate = p.EndDate,
            ProtestoPlaceIds = p.PlaceIds,
            GenderId = p.GenderId!.Value,
            Locations = ToLocations(p),
            EmployeeCountInProtesto = p.EmployeeCount,
            EmployeeCountInProtestoId = p.EmployeeCountRangeId,
            InterventionTypeIds = p.InterventionTypeIds,
            CustodyCount = p.CustodyCount,
            Note = p.Note,
        };

        private static List<ProtestoLocationModel> ToLocations(ProtestInput p) =>
            (p.Locations ?? []).Select(l => new ProtestoLocationModel { CityId = l.CityId, DistrictId = l.DistrictId, Place = l.Place }).ToList();
    }
}
