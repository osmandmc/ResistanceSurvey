using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;

namespace RS.MVC.Features.Definitions
{
    /// <summary>
    /// Text: part of the name. PendingOnly: values waiting for approval. TypeId: corporation type (Kurumlar only).
    /// UnusedOnly: companies no live case uses, as workplace or main company (Şirketler only).
    /// </summary>
    public record ListDefinitionsQuery(DefinitionKind Kind, string Text = null, bool PendingOnly = false, int? TypeId = null, int Page = 1,
        bool UnusedOnly = false);

    /// <summary>A page of one list, by name, with how many live cases (or protests) use each value.</summary>
    public class ListDefinitionsHandler(RSDBContext db)
    {
        public const int PageSize = 50;

        public async Task<DefinitionPage> Handle(ListDefinitionsQuery query)
        {
            var page = Math.Max(1, query.Page);
            var text = string.IsNullOrWhiteSpace(query.Text) ? null : query.Text.Trim();

            var (items, total, pending) = query.Kind switch
            {
                DefinitionKind.Companies => await CompaniesAsync(text, query.UnusedOnly, page),
                DefinitionKind.Corporations => await ApprovableAsync(
                    db.Corporation.Where(c => query.TypeId == null || c.CorporationTypeId == query.TypeId), text, query.PendingOnly, page,
                    c => new DefinitionItem(c.Id, c.Name, c.Approved, 0, c.CorporationTypeId, null, null, null, null,
                        c is TradeUnion ? ((TradeUnion)c).ConfederationId : null)),
                DefinitionKind.Reasons => await ApprovableAsync(db.ResistanceReason, text, query.PendingOnly, page,
                    r => new DefinitionItem(r.Id, r.Name, r.Approved, 0, null, null, null, null, null, null)),
                DefinitionKind.ProtestTypes => await ApprovableAsync(db.ProtestoType, text, query.PendingOnly, page,
                    t => new DefinitionItem(t.Id, t.Name, t.Approved, 0, null, t.AgainstProduction, t.Simple, null, null, null)),
                _ => await ApprovableAsync(db.ProtestoPlace, text, query.PendingOnly, page,
                    p => new DefinitionItem(p.Id, p.Name, p.Approved, 0, null, null, null, null, null, null)),
            };

            var usage = await DefinitionUsage.CountAsync(db, query.Kind, items.Select(i => i.Id).ToList());
            var withUsage = items.Select(i => i with { Usage = usage.GetValueOrDefault(i.Id) }).ToList();
            return new DefinitionPage(withUsage, total, page, PageSize, pending);
        }

        private async Task<(List<DefinitionItem>, int, int)> ApprovableAsync<T>(
            IQueryable<T> values, string text, bool pendingOnly, int page, System.Linq.Expressions.Expression<Func<T, DefinitionItem>> toItem)
            where T : LookupEntity, IApprovable
        {
            values = Filter(values, text, pendingOnly: false);
            var pending = await Filter(values, null, pendingOnly: true).CountAsync();
            values = Filter(values, null, pendingOnly);
            var total = await values.CountAsync();
            var items = await values.OrderBy(v => v.Name).ThenBy(v => v.Id)
                .Skip((page - 1) * PageSize).Take(PageSize).Select(toItem).ToListAsync();
            return (items, total, pending);
        }

        /// <summary>The list's filters (public so a test can check they translate to SQL Server).</summary>
        public static IQueryable<T> Filter<T>(IQueryable<T> values, string text, bool pendingOnly) where T : LookupEntity, IApprovable
        {
            if (text != null)
                values = values.Where(v => v.Name.Contains(text));
            return pendingOnly ? values.Where(v => !v.Approved) : values;
        }

        private async Task<(List<DefinitionItem>, int, int)> CompaniesAsync(string text, bool unusedOnly, int page)
        {
            var companies = Companies(db, text, unusedOnly);
            var total = await companies.CountAsync();
            var items = await companies.OrderBy(c => c.Name).ThenBy(c => c.Id)
                .Skip((page - 1) * PageSize).Take(PageSize)
                .Select(c => new DefinitionItem(c.Id, c.Name, null, 0, c.CompanyTypeId, null, null, c.CompanyScaleId, c.CompanyWorkLineId, null))
                .ToListAsync();
            return (items, total, 0);
        }

        /// <summary>
        /// The company list's filters (public so a test can check they translate to SQL Server). Unused: no live case
        /// uses the company, the same rule as the usage count, so companies of deleted cases count as unused.
        /// </summary>
        public static IQueryable<COMMON.Entities.Company> Companies(RSDBContext db, string text, bool unusedOnly)
        {
            var companies = db.Company.Where(c => !c.Deleted);
            if (text != null)
                companies = companies.Where(c => c.Name.Contains(text));
            if (unusedOnly)
                companies = companies.Where(c => !db.Resistance.Any(r => !r.Deleted && (r.CompanyId == c.Id || r.MainCompanyId == c.Id)));
            return companies;
        }
    }
}
