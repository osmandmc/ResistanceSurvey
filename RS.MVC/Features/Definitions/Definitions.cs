using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.EF;

namespace RS.MVC.Features.Definitions
{
    /// <summary>The lists researchers keep clean in Tanımlar (R12a). Unions are corporations of type Sendika.</summary>
    public enum DefinitionKind
    {
        Companies,
        Corporations,
        Reasons,
        ProtestTypes,
        ProtestPlaces,
    }

    public static class DefinitionKinds
    {
        private static readonly Dictionary<string, DefinitionKind> Slugs = new()
        {
            ["companies"] = DefinitionKind.Companies,
            ["corporations"] = DefinitionKind.Corporations,
            ["reasons"] = DefinitionKind.Reasons,
            ["protest-types"] = DefinitionKind.ProtestTypes,
            ["protest-places"] = DefinitionKind.ProtestPlaces,
        };

        public static bool TryParse(string slug, out DefinitionKind kind) => Slugs.TryGetValue(slug ?? "", out kind);

        public static string Slug(DefinitionKind kind) => Slugs.First(s => s.Value == kind).Key;

        /// <summary>One value of the kind, as the activity log names it (shown to users).</summary>
        public static string Label(DefinitionKind kind) => kind switch
        {
            DefinitionKind.Companies => "Şirket",
            DefinitionKind.Corporations => "Kurum",
            DefinitionKind.Reasons => "Vaka nedeni",
            DefinitionKind.ProtestTypes => "Eylem türü",
            _ => "Eylem mekânı",
        };

        /// <summary>The value's name (companies also when marked deleted), or null.</summary>
        public static async Task<string> NameAsync(RSDBContext db, DefinitionKind kind, int id) =>
            kind == DefinitionKind.Companies
                ? await db.Company.Where(c => c.Id == id).Select(c => c.Name).FirstOrDefaultAsync()
                : (await DefinitionValues.FindAsync(db, kind, id))?.Name;

        /// <summary>Companies have no approval: they are added on purpose with "Şirket Ekle".</summary>
        public static bool HasApproval(DefinitionKind kind) => kind != DefinitionKind.Companies;

        /// <summary>Protest types and places are used by protests; the others by cases.</summary>
        public static bool CountsProtests(DefinitionKind kind) => kind is DefinitionKind.ProtestTypes or DefinitionKind.ProtestPlaces;
    }

    /// <summary>
    /// One value of a list. Approved is null for companies. Usage: live cases (or protests) that use it.
    /// The other fields belong to one kind each and are null elsewhere: TypeId (corporation type, or company type),
    /// AgainstProduction and Simple (protest types), ScaleId and WorklineId (companies).
    /// </summary>
    public record DefinitionItem(
        int Id,
        string Name,
        bool? Approved,
        int Usage,
        int? TypeId = null,
        bool? AgainstProduction = null,
        bool? Simple = null,
        int? ScaleId = null,
        int? WorklineId = null,
        int? ConfederationId = null);

    public record DefinitionPage(IReadOnlyList<DefinitionItem> Items, int Total, int Page, int PageSize, int PendingCount);

    /// <summary>Which live cases (or protests) use each value; shared by the list and the merge preview.</summary>
    public static class DefinitionUsage
    {
        /// <summary>Id → number of live cases (protests for protest types and places) that use it.</summary>
        public static async Task<Dictionary<int, int>> CountAsync(RSDBContext db, DefinitionKind kind, IReadOnlyCollection<int> ids)
        {
            var pairs = await UsersAsync(db, kind, ids);
            return pairs.GroupBy(p => p.Id).ToDictionary(g => g.Key, g => g.Select(p => p.UserId).Distinct().Count());
        }

        /// <summary>(value id, case or protest id) for every live use.</summary>
        public static async Task<List<(int Id, int UserId)>> UsersAsync(RSDBContext db, DefinitionKind kind, IReadOnlyCollection<int> ids)
        {
            var list = ids.ToList();
            switch (kind)
            {
                case DefinitionKind.Companies:
                    var asCompany = await db.Resistance.Where(r => !r.Deleted && r.CompanyId != null && list.Contains(r.CompanyId.Value))
                        .Select(r => new { Id = r.CompanyId.Value, CaseId = r.Id }).ToListAsync();
                    var asMain = await db.Resistance.Where(r => !r.Deleted && r.MainCompanyId != null && list.Contains(r.MainCompanyId.Value))
                        .Select(r => new { Id = r.MainCompanyId.Value, CaseId = r.Id }).ToListAsync();
                    return asCompany.Concat(asMain).Select(x => (x.Id, x.CaseId)).ToList();
                case DefinitionKind.Corporations:
                    var supporting = await db.ResistanceCorporation.Where(x => list.Contains(x.CorporationId) && !x.Resistance.Deleted)
                        .Select(x => new { Id = x.CorporationId, CaseId = x.ResistanceId }).ToListAsync();
                    var union = await db.Resistance.Where(r => !r.Deleted && r.TradeUnionId != null && list.Contains(r.TradeUnionId.Value))
                        .Select(r => new { Id = r.TradeUnionId.Value, CaseId = r.Id }).ToListAsync();
                    return supporting.Concat(union).Select(x => (x.Id, x.CaseId)).ToList();
                case DefinitionKind.Reasons:
                    return (await db.ResistanceResistanceReason.Where(x => list.Contains(x.ResistanceReasonId) && !x.Resistance.Deleted)
                        .Select(x => new { Id = x.ResistanceReasonId, x.ResistanceId }).ToListAsync()).Select(x => (x.Id, x.ResistanceId)).ToList();
                case DefinitionKind.ProtestTypes:
                    return (await db.ProtestoProtestoType.Where(x => list.Contains(x.ProtestoTypeId) && !x.Protesto.Deleted && !x.Protesto.Resistance.Deleted)
                        .Select(x => new { Id = x.ProtestoTypeId, x.ProtestoId }).ToListAsync()).Select(x => (x.Id, x.ProtestoId)).ToList();
                default:
                    return (await db.ProtestoProtestoPlace.Where(x => list.Contains(x.ProtestoPlaceId) && !x.Protesto.Deleted && !x.Protesto.Resistance.Deleted)
                        .Select(x => new { Id = x.ProtestoPlaceId, x.ProtestoId }).ToListAsync()).Select(x => (x.Id, x.ProtestoId)).ToList();
            }
        }
    }
}
