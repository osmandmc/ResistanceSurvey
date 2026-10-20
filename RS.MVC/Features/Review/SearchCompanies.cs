using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.EF;

namespace RS.MVC.Features.Review
{
    /// <summary>Companies by name, for the case form (instead of loading every company).</summary>
    public record SearchCompaniesQuery(string Text);

    public class SearchCompaniesHandler(RSDBContext db)
    {
        public const int MaxResults = 20;

        public async Task<IReadOnlyList<Option>> Handle(SearchCompaniesQuery query)
        {
            var text = query.Text?.Trim() ?? "";
            return await db.Company
                .Where(c => !c.Deleted && (text == "" || c.Name.Contains(text)))
                .OrderBy(c => c.Name)
                .Take(MaxResults)
                .Select(c => new Option(c.Id, c.Name))
                .ToListAsync();
        }
    }
}
