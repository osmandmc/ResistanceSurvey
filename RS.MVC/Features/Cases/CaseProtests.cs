using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.EF;

namespace RS.MVC.Features.Cases
{
    /// <summary>One protest as a line: dates, types, "city / district · place", and its short description if any.</summary>
    public record ProtestLine(int Id, DateTime StartDate, DateTime? EndDate, IReadOnlyList<string> Types, IReadOnlyList<string> Places, string Description);

    /// <summary>Vakalar: the protests of a case, newest first, shown when its row is opened.</summary>
    public class CaseProtestLinesHandler(RSDBContext db)
    {
        public async Task<IReadOnlyList<ProtestLine>> Handle(int caseId)
        {
            var rows = await db.Protesto.AsNoTracking()
                .Where(p => p.ResistanceId == caseId && !p.Deleted && !p.Resistance.Deleted)
                .OrderByDescending(p => p.StartDate).ThenByDescending(p => p.Id)
                .Select(p => new
                {
                    p.Id, p.StartDate, p.EndDate, p.SimpleProtestoDescription,
                    Types = p.ProtestoProtestoTypes.Select(t => t.ProtestoType.Name).ToList(),
                    Locations = p.Locations.Select(l => new { City = l.City.Name, District = l.District != null ? l.District.Name : null, l.Place }).ToList(),
                })
                .ToListAsync();
            return rows.Select(p => new ProtestLine(
                    p.Id, p.StartDate, p.EndDate, p.Types,
                    p.Locations.Select(l => string.Join(" · ", new[] { l.District != null ? $"{l.City} / {l.District}" : l.City, l.Place }
                        .Where(s => !string.IsNullOrWhiteSpace(s)))).ToList(),
                    string.IsNullOrWhiteSpace(p.SimpleProtestoDescription) ? null : p.SimpleProtestoDescription.Trim()))
                .ToList();
        }
    }
}
