using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.EF;
using RS.MVC.Features.Review;

namespace RS.MVC.Features.Inbox
{
    /// <summary>
    /// An article of the same days, already linked to a case, that looks like the same event (R29). Reason: "aynı şirket:
    /// X" or "aynı örgüt ve il: X, Y".
    /// </summary>
    public record InboxSibling(int CaseId, string CaseName, string Title, string Reason);

    /// <summary>
    /// "Benzer bir haber Vaka X'e bağlandı" (R29): for open inbox articles, an article linked to a case within
    /// <see cref="Days"/> days whose case names the same company (or main company) in the open article's title or draft;
    /// for a case without a company, one of its organizations together with a province of its protests. So a second
    /// outlet's report is handled with the first one and does not become a second protest.
    /// </summary>
    public class SiblingFinder(RSDBContext db)
    {
        public const int Days = 2;

        private record LinkedCase(int CaseId, string CaseName, string Title, DateTime Date, string Company, string MainCompany,
            List<string> Organizations, List<string> Cities);

        /// <summary>Article id → its sibling, for the open articles that have one.</summary>
        public async Task<IReadOnlyDictionary<string, InboxSibling>> FindAsync(IReadOnlyList<InboxItem> open)
        {
            var dated = open.Where(i => i.PublishedAt != null).ToList();
            if (dated.Count == 0)
                return new Dictionary<string, InboxSibling>();
            // News.Date is kept in Turkey time.
            var from = dated.Min(i => TurkeyDay(i.PublishedAt!.Value)).AddDays(-Days);
            var to = dated.Max(i => TurkeyDay(i.PublishedAt!.Value)).AddDays(Days + 1);

            var linked = (await db.ResistanceNews
                    .Where(rn => !rn.Resistance.Deleted && rn.News.Date >= from && rn.News.Date < to)
                    .Select(rn => new
                    {
                        rn.ResistanceId,
                        rn.Resistance.Name,
                        rn.News.Header,
                        rn.News.Date,
                        Company = rn.Resistance.Company.Name,
                        MainCompany = rn.Resistance.MainCompany.Name,
                        Organizations = rn.Resistance.ResistanceCorporations.Select(c => c.Corporation.Name).ToList(),
                        Cities = rn.Resistance.Protestos.Where(p => !p.Deleted).SelectMany(p => p.Locations).Select(l => l.City.Name).Distinct().ToList(),
                    })
                    .ToListAsync())
                .Select(x => new LinkedCase(x.ResistanceId, x.Name, x.Header, x.Date, x.Company, x.MainCompany, x.Organizations ?? [], x.Cities ?? []))
                .ToList();

            var siblings = new Dictionary<string, InboxSibling>();
            foreach (var item in dated)
            {
                var text = CompanyNameMatcher.Normalize($"{item.Title} {item.Draft}");
                var day = TurkeyDay(item.PublishedAt!.Value);
                var near = linked.Where(l => Math.Abs((l.Date.Date - day).TotalDays) <= Days).OrderBy(l => Math.Abs((l.Date.Date - day).TotalDays));
                var sibling = near.Select(l => Match(l, text)).FirstOrDefault(s => s != null);
                if (sibling != null)
                    siblings[item.ArticleId] = sibling;
            }
            return siblings;
        }

        private static InboxSibling Match(LinkedCase linked, string text)
        {
            var name = linked.CaseName ?? linked.Company ?? linked.MainCompany ?? $"#{linked.CaseId}";
            foreach (var company in new[] { linked.Company, linked.MainCompany })
            {
                if (Contains(text, CompanyNameMatcher.Core(company)))
                    return new InboxSibling(linked.CaseId, name, linked.Title, $"aynı şirket: {company}");
            }
            // An organization alone says little (DİSK has many cases in İstanbul): only with a province of the case.
            if (linked.Company == null && linked.MainCompany == null)
            {
                var organization = linked.Organizations.FirstOrDefault(o => Contains(text, CompanyNameMatcher.Core(o)));
                var city = linked.Cities.FirstOrDefault(c => Contains(text, CompanyNameMatcher.Normalize(c).Trim()));
                if (organization != null && city != null)
                    return new InboxSibling(linked.CaseId, name, linked.Title, $"aynı örgüt ve il: {organization}, {city}");
            }
            return null;
        }

        private static DateTime TurkeyDay(DateTimeOffset value) => value.ToOffset(TimeSpan.FromHours(3)).Date;

        private static bool Contains(string normalizedText, string core) =>
            !string.IsNullOrEmpty(core) && normalizedText.Contains($" {core} ", StringComparison.Ordinal);
    }
}
