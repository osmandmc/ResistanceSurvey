using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.EF;
using RS.MVC.Models;

namespace RS.MVC.Features.Review
{
    /// <summary>Existing resistances whose name or company matches, for adding a protest or linking.</summary>
    public record SearchCasesQuery(string Text);

    public class SearchCasesHandler(RSDBContext db)
    {
        public const int MaxResults = 20;

        /// <summary>Most recent protest first; deleted cases are left out.</summary>
        public async Task<IReadOnlyList<ResistanceOption>> Handle(SearchCasesQuery query)
        {
            var text = query.Text?.Trim() ?? "";
            return await db.Resistance
                .Where(r => !r.Deleted)
                .Where(r => text == "" || r.Name.Contains(text) || (r.Company != null && r.Company.Name.Contains(text)))
                .Select(r => new ResistanceOption(
                    r.Id,
                    r.Name,
                    r.Company != null ? r.Company.Name : null,
                    r.Protestos.Where(p => !p.Deleted).Max(p => (DateTime?)p.StartDate)))
                .OrderByDescending(o => o.LastProtestDate)
                .Take(MaxResults)
                .ToListAsync();
        }
    }
}
