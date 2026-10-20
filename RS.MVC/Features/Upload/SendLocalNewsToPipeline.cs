using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RS.EF;
using RS.MVC.Applications;
using RS.MVC.Models;
using static RS.COMMON.Constants.Enums;

namespace RS.MVC.Features.Upload
{
    /// <summary>Per year: how many pending local news items there are, and how many of them can go (a pipeline source with a link).</summary>
    public record LocalNewsYear(int Year, int Pending, int Sendable);

    /// <summary>
    /// Sent: rows offered to the pipeline in this run. NextSkip: pass it as skip to continue; null when everything was sent.
    /// </summary>
    public record LocalNewsTransfer(
        IReadOnlyList<LocalNewsYear> Years,
        int Total,
        int Sent,
        int Accepted,
        int Duplicate,
        IReadOnlyList<string> Rejected,
        int? NextSkip,
        bool DryRun);

    /// <summary>
    /// One-off for the move to the redesign (R12b, run at R14 on the database that stays): the news imported into the app
    /// the old way and never triaged (Pending, no ArticleId) go to the pipeline with their text, so they appear in the inbox
    /// under their own month and are triaged with İnceleme. The rows stay as they are: linking or dismissing an article finds
    /// its old row by link (ArticleNewsService). Safe to repeat: articles the pipeline already has come back as Duplicate.
    /// </summary>
    public class SendLocalNewsToPipelineHandler(RSDBContext db, IPipelineClient pipeline, IOptions<PipelineSettings> settings)
    {
        public const int MaxPerRun = 2000;

        /// <param name="skip">Sendable rows (newest first) already sent by earlier runs.</param>
        public async Task<LocalNewsTransfer> Handle(bool dryRun, int skip = 0, int limit = MaxPerRun)
        {
            var sources = settings.Value.Sources.Keys.ToList();
            var pending = db.News.Where(n => n.Status == Status.Pending && n.ArticleId == null);
            var sendable = pending.Where(n => sources.Contains(n.Source) && n.Link != null && n.Link != "");

            var years = (await pending
                    .GroupBy(n => n.Date.Year)
                    .Select(g => new { Year = g.Key, Pending = g.Count(), Sendable = g.Count(n => sources.Contains(n.Source) && n.Link != null && n.Link != "") })
                    .ToListAsync())
                .OrderByDescending(y => y.Year)
                .Select(y => new LocalNewsYear(y.Year, y.Pending, y.Sendable))
                .ToList();
            var total = years.Sum(y => y.Sendable);
            if (dryRun)
                return new LocalNewsTransfer(years, total, 0, 0, 0, [], 0, true);

            // Newest first: the recent months are the most useful to triage. A run sends at most MaxPerRun rows, so each call
            // stays well within the web server's time limit; the next run continues with skip = NextSkip. The order is
            // stable because sending changes nothing in the app's rows.
            skip = System.Math.Max(0, skip);
            var rows = await sendable.OrderByDescending(n => n.Date).ThenByDescending(n => n.Id)
                .Skip(skip).Take(System.Math.Clamp(limit, 1, MaxPerRun)).ToListAsync();
            var results = await pipeline.SubmitAsync(rows.Select(UploadNewsHandler.ToPipelineArticle).ToList(), "Csv");
            var next = skip + rows.Count;
            return new LocalNewsTransfer(
                years,
                total,
                rows.Count,
                results.Count(r => r.Status == "Accepted"),
                results.Count(r => r.Status == "Duplicate"),
                results.Where(r => r.Status == "Rejected").Select(r => $"{r.Url}: {r.Reason}").Take(20).ToList(),
                next < total ? next : null,
                false);
        }
    }
}
