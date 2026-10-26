using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.EF;

namespace RS.MVC.Features.Review
{
    /// <summary>
    /// The list values the extraction prompt offers, from the database: for the prompt researchers copy ("AI ile doldur")
    /// and for the pipeline's drafts (R30). Key: the pipeline's list name; Title: the heading in the prompt.
    /// </summary>
    public static class AiLists
    {
        public record AiList(string Key, string Title, IReadOnlyList<string> Values);

        public static async Task<IReadOnlyList<AiList>> LoadAsync(RSDBContext db) =>
        [
            new("categories", "Kategoriler", Clean(await db.Category.Select(e => e.Name).ToListAsync())),
            new("reasons", "Direniş nedenleri", Clean(await db.ResistanceReason.Select(e => e.Name).ToListAsync())),
            new("employmentTypes", "İstihdam biçimleri", Clean(await db.EmploymentType.Select(e => e.Name).ToListAsync())),
            new("tradeUnionAuthorities", "Sendika yetkisi", Clean(await db.TradeUnionAuthority.Select(e => e.Name).ToListAsync())),
            new("protestTypes", "Eylem türleri", Clean(await db.ProtestoType.Select(e => e.Name).ToListAsync())),
            new("protestPlaces", "Eylem yerleri", Clean(await db.ProtestoPlace.Select(e => e.Name).ToListAsync())),
            new("interventionTypes", "Müdahaleler", Clean(await db.InterventionType.Select(e => e.Name).ToListAsync())),
            new("cities", "İller", Clean(await db.City.Select(e => e.Name).ToListAsync())),
        ];

        /// <summary>Trimmed, no blanks or duplicates, in Turkish order.</summary>
        private static List<string> Clean(IEnumerable<string> names) =>
            names.Where(n => !string.IsNullOrWhiteSpace(n)).Select(n => n.Trim()).Distinct()
                .OrderBy(n => n, System.StringComparer.Create(new System.Globalization.CultureInfo("tr-TR"), ignoreCase: true)).ToList();
    }
}
