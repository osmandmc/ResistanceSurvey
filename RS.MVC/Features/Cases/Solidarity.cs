using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.COMMON.Constants;
using RS.COMMON.Entities;
using RS.EF;
using RS.MVC.Features.Activity;
using RS.MVC.Features.Review;

namespace RS.MVC.Features.Cases
{
    /// <summary>A case offered as the supported one, and why: "aynı işyeri", or the workplace named in the group.</summary>
    public record SupportedSuggestion(int CaseId, string Name, string CompanyName, DateTime? LastProtestDate, bool Ongoing, string Reason);

    /// <summary>
    /// Old Dayanışma cases (R31 migration) that shared one "company" name, still to be dealt with. OldCompanyUsedByIsletme:
    /// the name is a real workplace with İşletme cases. LooksLikeOrganization: an organization of that name exists.
    /// </summary>
    public record SolidarityGroup(
        string Name,
        int? OldCompanyId,
        IReadOnlyList<int> CaseIds,
        IReadOnlyList<string> Cities,
        DateTime? FirstDate,
        DateTime? LastDate,
        bool OldCompanyUsedByIsletme,
        bool LooksLikeOrganization,
        SupportedSuggestion Suggestion);

    /// <summary>An İşletme case that had a "Hedef kurum" before R31: a researcher checks it.</summary>
    public record TargetCheck(int CaseId, string Name, string CompanyName, string OldTargetName);

    public record SolidarityLinksPage(IReadOnlyList<SolidarityGroup> Groups, IReadOnlyList<TargetCheck> TargetChecks, int UnlinkedNewCases);

    /// <summary>
    /// "Dayanışma bağlantıları" (R31c): the old Dayanışma cases grouped by the name their company field held, each with a
    /// suggested supported case (the same workplace's İşletme case nearest in time, or one whose workplace the name
    /// mentions); and the İşletme cases with an old target institution.
    /// </summary>
    public class ListSolidarityLinksHandler(RSDBContext db)
    {
        public async Task<SolidarityLinksPage> Handle()
        {
            var open = await db.CaseMigrationRecord.AsNoTracking()
                .Where(m => m.Step == "solidarity" && m.ResolvedAt == null)
                .Join(db.Resistance.Where(r => !r.Deleted && r.SupportedResistanceId == null), m => m.ResistanceId, r => r.Id,
                    (m, r) => new { r.Id, m.OldCompanyId, m.OldCompanyName })
                .ToListAsync();
            var caseIds = open.Select(o => o.Id).ToList();
            var protests = await db.Protesto.AsNoTracking()
                .Where(p => caseIds.Contains(p.ResistanceId) && !p.Deleted)
                .Select(p => new { p.ResistanceId, p.StartDate, Cities = p.Locations.Select(l => l.City.Name).ToList() })
                .ToListAsync();

            var workplaces = await WorkplaceCasesAsync();
            var companies = workplaces.Where(w => w.CompanyId != null).Select(w => (w.CompanyId!.Value, w.CompanyName)).Distinct().ToList();
            var organizationNames = (await db.Corporation.AsNoTracking().Select(c => c.Name).ToListAsync())
                .Where(n => n != null).Select(NameMatching.Key).ToHashSet();

            var groups = open
                .GroupBy(o => NameMatching.Clean(o.OldCompanyName ?? ""))
                .Select(g =>
                {
                    var ids = g.Select(o => o.Id).ToList();
                    var own = protests.Where(p => ids.Contains(p.ResistanceId)).ToList();
                    var first = own.Count == 0 ? (DateTime?)null : own.Min(p => p.StartDate);
                    var last = own.Count == 0 ? (DateTime?)null : own.Max(p => p.StartDate);
                    var oldCompanyId = g.Select(o => o.OldCompanyId).FirstOrDefault(id => id != null);
                    var usedByIsletme = oldCompanyId != null && workplaces.Any(w => w.CompanyId == oldCompanyId || w.MainCompanyId == oldCompanyId);
                    return new SolidarityGroup(
                        g.Key,
                        oldCompanyId,
                        ids,
                        own.SelectMany(p => p.Cities).Where(c => c != null).GroupBy(c => c).OrderByDescending(c => c.Count()).Select(c => c.Key).Take(5).ToList(),
                        first,
                        last,
                        usedByIsletme,
                        g.Key != "" && organizationNames.Contains(NameMatching.Key(g.Key)),
                        Suggest(g.Key, oldCompanyId, usedByIsletme, first, last, workplaces, companies));
                })
                .OrderByDescending(g => g.CaseIds.Count)
                .ThenBy(g => g.Name, StringComparer.Create(System.Globalization.CultureInfo.GetCultureInfo("tr-TR"), true))
                .ToList();

            var checks = await db.CaseMigrationRecord.AsNoTracking()
                .Where(m => m.Step == "workplace-target" && m.ResolvedAt == null)
                .Join(db.Resistance.Where(r => !r.Deleted), m => m.ResistanceId, r => r.Id, (m, r) => new { m, r })
                .Select(x => new TargetCheck(
                    x.r.Id, x.r.Name,
                    x.r.Company != null ? x.r.Company.Name : null,
                    db.Corporation.Where(c => c.Id == x.m.OldCorporationId).Select(c => c.Name).FirstOrDefault()))
                .ToListAsync();

            var recorded = db.CaseMigrationRecord.Select(m => m.ResistanceId);
            var unlinkedNew = await db.Resistance.CountAsync(r =>
                !r.Deleted && r.CategoryId == KnownListIds.SolidarityCategory && r.SupportedResistanceId == null && !recorded.Contains(r.Id));
            return new SolidarityLinksPage(groups, checks, unlinkedNew);
        }

        internal record WorkplaceCase(int Id, string Name, int? CompanyId, int? MainCompanyId, string CompanyName, DateTime? LastProtestDate, DateTime? FirstProtestDate, bool Ongoing);

        /// <summary>Live İşletme cases with their dates.</summary>
        internal Task<List<WorkplaceCase>> WorkplaceCasesAsync() =>
            db.Resistance.AsNoTracking()
                .Where(r => !r.Deleted && r.CategoryId == KnownListIds.WorkplaceCategory)
                .Select(r => new WorkplaceCase(
                    r.Id, r.Name, r.CompanyId, r.MainCompanyId, r.Company != null ? r.Company.Name : null,
                    r.Protestos.Where(p => !p.Deleted).Max(p => (DateTime?)p.StartDate),
                    r.Protestos.Where(p => !p.Deleted).Min(p => (DateTime?)p.StartDate),
                    r.Protestos.Where(p => !p.Deleted).OrderByDescending(p => p.StartDate).Select(p => p.EndDate == null).FirstOrDefault()))
                .ToListAsync();

        /// <summary>The same workplace's case, else one whose workplace the group's name mentions; the nearest in time.</summary>
        internal static SupportedSuggestion Suggest(string name, int? oldCompanyId, bool usedByIsletme, DateTime? first, DateTime? last,
            IReadOnlyList<WorkplaceCase> workplaces, IReadOnlyList<(int Id, string Name)> companies)
        {
            IEnumerable<WorkplaceCase> candidates;
            string reason;
            if (usedByIsletme)
            {
                candidates = workplaces.Where(w => w.CompanyId == oldCompanyId || w.MainCompanyId == oldCompanyId);
                reason = "aynı işyeri";
            }
            else
            {
                var named = CompanyNameMatcher.Find(companies, name, "").Select(m => m.CompanyId).ToHashSet();
                candidates = workplaces.Where(w => w.CompanyId is { } id && named.Contains(id));
                reason = "adında işyeri geçiyor";
            }
            var best = candidates
                .OrderBy(w => Distance(w, first, last))
                .ThenByDescending(w => w.LastProtestDate)
                .FirstOrDefault();
            return best == null ? null
                : new SupportedSuggestion(best.Id, best.Name ?? best.CompanyName, best.CompanyName, best.LastProtestDate, best.Ongoing, reason);
        }

        // Days between the case's protests and the group's: 0 when they overlap.
        private static double Distance(WorkplaceCase w, DateTime? first, DateTime? last)
        {
            if (w.FirstProtestDate is not { } wf || w.LastProtestDate is not { } wl || first is not { } gf || last is not { } gl)
                return double.MaxValue;
            if (wl < gf) return (gf - wl).TotalDays;
            if (gl < wf) return (wf - gl).TotalDays;
            return 0;
        }
    }

    /// <summary>Link: the supported case for every case; Organizer: the name becomes an organizing organization; Workplace: the old company becomes the acting workplace again; None: left empty.</summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter<SolidarityAction>))]
    public enum SolidarityAction
    {
        Link,
        Organizer,
        Workplace,
        None,
    }

    public record ResolveSolidarityCommand(IReadOnlyList<int> CaseIds, SolidarityAction Action, int? SupportedCaseId = null);

    /// <summary>One choice for a group of old Dayanışma cases; marks their migration record as dealt with.</summary>
    public class ResolveSolidarityHandler(RSDBContext db, ListValueResolver lists)
    {
        /// <summary>Why it cannot be done, or null.</summary>
        public async Task<string> Handle(ResolveSolidarityCommand command)
        {
            var ids = (command.CaseIds ?? []).Distinct().ToList();
            var records = await db.CaseMigrationRecord
                .Where(m => ids.Contains(m.ResistanceId) && m.Step == "solidarity" && m.ResolvedAt == null)
                .ToListAsync();
            if (records.Count == 0)
                return "Bu vakalar için yapılacak bir şey kalmadı.";
            var caseIds = records.Select(m => m.ResistanceId).Distinct().ToList();
            var cases = await db.Resistance.Where(r => caseIds.Contains(r.Id) && !r.Deleted && r.CategoryId == KnownListIds.SolidarityCategory).ToListAsync();
            var name = NameMatching.Clean(records.First().OldCompanyName ?? "");
            string summary;

            switch (command.Action)
            {
                case SolidarityAction.Link:
                    if (command.SupportedCaseId is not { } supported || caseIds.Contains(supported)
                        || !await db.Resistance.AnyAsync(r => r.Id == supported && !r.Deleted))
                        return "Desteklenen vaka bulunamadı.";
                    foreach (var r in cases)
                        r.SupportedResistanceId = supported;
                    var supportedName = await db.Resistance.Where(r => r.Id == supported)
                        .Select(r => r.Name ?? (r.Company != null ? r.Company.Name : null)).FirstOrDefaultAsync();
                    summary = $"{ActivityLog.Quoted(name)} ({cases.Count} vaka) → {ActivityLog.Quoted(supportedName ?? $"Vaka #{supported}")}";
                    break;
                case SolidarityAction.Organizer:
                    if (name == "")
                        return "Grubun adı yok.";
                    var organizationId = (await lists.CorporationIdsAsync([], [name])).Single();
                    foreach (var r in cases)
                    {
                        if (!await db.ResistanceCorporation.AnyAsync(x => x.ResistanceId == r.Id && x.CorporationId == organizationId))
                            db.ResistanceCorporation.Add(new ResistanceCorporation { ResistanceId = r.Id, CorporationId = organizationId, Role = OrganizationRole.Organizing });
                    }
                    summary = $"{ActivityLog.Quoted(name)} ({cases.Count} vaka): örgütleyen kurum";
                    break;
                case SolidarityAction.Workplace:
                    var companyId = records.Select(m => m.OldCompanyId).FirstOrDefault(id => id != null);
                    var company = companyId is { } cid ? await db.Company.FirstOrDefaultAsync(c => c.Id == cid) : null;
                    if (company == null)
                        return "Eski işyeri bulunamadı.";
                    company.Deleted = false;
                    foreach (var r in cases)
                        r.CompanyId = company.Id;
                    summary = $"{ActivityLog.Quoted(name)} ({cases.Count} vaka): eylemi yapan işyeri";
                    break;
                default:
                    summary = $"{ActivityLog.Quoted(name)} ({cases.Count} vaka): boş bırakıldı";
                    break;
            }

            var now = DateTime.UtcNow;
            foreach (var m in records)
                m.ResolvedAt = now;
            db.LogActivity(Actions.SolidarityResolve, summary);
            await db.SaveChangesAsync();
            return null;
        }
    }

    /// <summary>An İşletme case with an old "Hedef kurum" was checked by a researcher.</summary>
    public class ResolveTargetCheckHandler(RSDBContext db)
    {
        public async Task<bool> Handle(int caseId)
        {
            var records = await db.CaseMigrationRecord.Where(m => m.ResistanceId == caseId && m.Step == "workplace-target" && m.ResolvedAt == null).ToListAsync();
            if (records.Count == 0)
                return false;
            foreach (var m in records)
                m.ResolvedAt = DateTime.UtcNow;
            db.LogActivity(Actions.TargetCheck, $"Vaka #{caseId}: eski hedef kurum kontrol edildi", caseId);
            await db.SaveChangesAsync();
            return true;
        }
    }

    /// <summary>On a case's page (R31c): the Dayanışma cases linked to it, and unlinked ones that name its workplace.</summary>
    public record CaseSolidarity(IReadOnlyList<CaseLink> Linked, IReadOnlyList<CaseLink> Suggested);

    public class CaseSolidarityHandler(RSDBContext db)
    {
        public const int MaxSuggestions = 10;

        public async Task<CaseSolidarity> Handle(int caseId)
        {
            var linked = await db.Resistance.AsNoTracking()
                .Where(r => !r.Deleted && r.SupportedResistanceId == caseId)
                .OrderByDescending(r => r.Id)
                .Select(r => new CaseLink(r.Id, r.Name))
                .ToListAsync();

            var workplace = await db.Resistance.AsNoTracking()
                .Where(r => r.Id == caseId && !r.Deleted && r.CategoryId == KnownListIds.WorkplaceCategory && r.Company != null)
                .Select(r => new { r.CompanyId, CompanyName = r.Company.Name })
                .FirstOrDefaultAsync();
            if (workplace == null)
                return new CaseSolidarity(linked, []);

            var unlinked = await db.Resistance.AsNoTracking()
                .Where(r => !r.Deleted && r.CategoryId == KnownListIds.SolidarityCategory && r.SupportedResistanceId == null && r.Id != caseId)
                .Select(r => new { r.Id, r.Name, r.Description })
                .ToListAsync();
            var company = new[] { (workplace.CompanyId!.Value, workplace.CompanyName) };
            var suggested = unlinked
                .Where(r => CompanyNameMatcher.Find(company, r.Name, r.Description).Count > 0)
                .OrderByDescending(r => r.Id)
                .Take(MaxSuggestions)
                .Select(r => new CaseLink(r.Id, r.Name))
                .ToList();
            return new CaseSolidarity(linked, suggested);
        }
    }

    public record LinkSupportedCaseCommand(int SolidarityCaseId, int SupportedCaseId);

    /// <summary>"Bağla" on a case's page: the Dayanışma case gets this supported case.</summary>
    public class LinkSupportedCaseHandler(RSDBContext db)
    {
        /// <summary>Why not, or null.</summary>
        public async Task<string> Handle(LinkSupportedCaseCommand command)
        {
            if (command.SolidarityCaseId == command.SupportedCaseId)
                return "Bir vaka kendisini destekleyemez.";
            var solidarity = await db.Resistance.FirstOrDefaultAsync(r => r.Id == command.SolidarityCaseId && !r.Deleted);
            if (solidarity is not { CategoryId: KnownListIds.SolidarityCategory })
                return "Dayanışma vakası bulunamadı.";
            var supportedName = await db.Resistance.Where(r => r.Id == command.SupportedCaseId && !r.Deleted).Select(r => r.Name).FirstOrDefaultAsync();
            if (!await db.Resistance.AnyAsync(r => r.Id == command.SupportedCaseId && !r.Deleted))
                return "Desteklenen vaka bulunamadı.";
            solidarity.SupportedResistanceId = command.SupportedCaseId;
            db.LogActivity(Actions.CaseUpdate, $"{ActivityLog.Quoted(solidarity.Name)} → desteklenen vaka {ActivityLog.Quoted(supportedName ?? $"#{command.SupportedCaseId}")}", solidarity.Id);
            await db.SaveChangesAsync();
            return null;
        }
    }
}
