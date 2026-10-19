using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.COMMON.Entities;
using RS.EF;
using static RS.COMMON.Constants.Enums;

namespace RS.MVC.Features.Cases
{
    /// <summary>
    /// Articles already in the app (News rows), for attaching to a case on its page. Articles still waiting in the
    /// pipeline are attached from the inbox's review screen instead.
    /// </summary>
    public record SearchNewsQuery(string Text, int? Year, int? Month);

    public record NewsOption(int Id, string Title, string Link, DateTime Date, bool Linked);

    public class SearchNewsHandler(RSDBContext db)
    {
        public const int MaxResults = 30;

        public async Task<IReadOnlyList<NewsOption>> Handle(SearchNewsQuery query)
        {
            var text = query.Text?.Trim() ?? "";
            var news = db.News.AsQueryable();
            if (text != "")
                news = news.Where(n => n.Header.Contains(text) || n.Link.Contains(text));
            if (query.Year is { } year)
                news = news.Where(n => n.Date.Year == year && (query.Month == null || n.Date.Month == query.Month));
            return await news
                .OrderByDescending(n => n.Date)
                .Take(MaxResults)
                .Select(n => new NewsOption(n.Id, n.Header, n.Link, n.Date, n.Status == Status.Linked))
                .ToListAsync();
        }
    }

    public record AttachArticleCommand(int CaseId, int NewsId);

    /// <summary>Links an existing News row to the case and marks it Linked (as the old news page's ✓ did).</summary>
    public class AttachArticleHandler(RSDBContext db)
    {
        public async Task<CaseCommandResult> Handle(AttachArticleCommand command)
        {
            var news = await db.News.FirstOrDefaultAsync(n => n.Id == command.NewsId);
            if (news == null || !await db.Resistance.AnyAsync(r => r.Id == command.CaseId && !r.Deleted))
                return CaseCommandResult.NotFound;
            if (!await db.ResistanceNews.AnyAsync(rn => rn.ResistanceId == command.CaseId && rn.NewsId == command.NewsId))
                db.ResistanceNews.Add(new ResistanceNews { ResistanceId = command.CaseId, NewsId = command.NewsId });
            news.Status = Status.Linked;
            await db.SaveChangesAsync();
            return CaseCommandResult.Ok(command.NewsId);
        }
    }

    public record DetachArticleCommand(int CaseId, int NewsId);

    /// <summary>Removes the article from the case. Its News row stays (and stays handled for the inbox).</summary>
    public class DetachArticleHandler(RSDBContext db)
    {
        public async Task<CaseCommandResult> Handle(DetachArticleCommand command)
        {
            var links = await db.ResistanceNews.Where(rn => rn.ResistanceId == command.CaseId && rn.NewsId == command.NewsId).ToListAsync();
            if (links.Count == 0)
                return CaseCommandResult.NotFound;
            db.ResistanceNews.RemoveRange(links);
            await db.SaveChangesAsync();
            return CaseCommandResult.Ok(command.NewsId);
        }
    }
}
