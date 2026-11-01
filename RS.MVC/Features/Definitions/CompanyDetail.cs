using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.COMMON.Constants;
using RS.COMMON.Entities;
using RS.EF;
using RS.MVC.Features.Activity;
using RS.MVC.Features.Cases;

namespace RS.MVC.Features.Definitions
{
    /// <summary>A case using the company. AsMainCompany: the company is the organization above the case's employer.</summary>
    public record CompanyCase(int Id, string Name, DateTime StartDate, bool AsMainCompany);

    /// <summary>
    /// A fact between this company and another (R37, part B), seen from this company. Outgoing: “this → other” (this is
    /// part of the group other, belongs to the public body other, was renamed to other); incoming: the reverse (other is
    /// one of this group's companies, belongs to this body, is this company's former name).
    /// </summary>
    public record CompanyRelationItem(
        int Id, CompanyRelationKind Kind, bool Outgoing, int OtherId, string OtherName,
        decimal? SharePercent, DateTime? StartDate, DateTime? EndDate, string SourceUrl, string Note);

    /// <summary>
    /// A link that depends on the job, read from the cases (R37: decision b): the role of the case's employer, the other
    /// company, how many live İşletme cases and their first and last protest. AsEmployer: this company is the employer
    /// (the other is above it); else this company is above (the other is the employer).
    /// </summary>
    public record CompanyJobLink(EmployerRole Role, bool AsEmployer, int OtherId, string OtherName, int Cases, DateTime? First, DateTime? Last);

    /// <summary>One company in Tanımlar: its fields, its live cases, its facts with other companies and its links from cases.</summary>
    public record CompanyDetail(
        int Id,
        string Name,
        int? TypeId,
        int? ScaleId,
        int? WorklineId,
        InstitutionKind? InstitutionKind,
        int? ServiceBranchId,
        IReadOnlyList<CompanyCase> Cases,
        IReadOnlyList<CompanyRelationItem> Relations,
        IReadOnlyList<CompanyJobLink> JobLinks);

    public class GetCompanyHandler(RSDBContext db)
    {
        /// <summary>Null when there is no such (undeleted) company.</summary>
        public async Task<CompanyDetail> Handle(int id)
        {
            var company = await db.Company.Where(c => c.Id == id && !c.Deleted)
                .Select(c => new { c.Id, c.Name, c.CompanyTypeId, c.CompanyScaleId, c.CompanyWorkLineId, c.InstitutionKind, c.ServiceBranchId })
                .FirstOrDefaultAsync();
            if (company == null)
                return null;

            var cases = await db.Resistance
                .Where(r => !r.Deleted && (r.CompanyId == id || r.MainCompanyId == id))
                .OrderByDescending(r => r.StartDate).ThenByDescending(r => r.Id)
                .Select(r => new CompanyCase(r.Id, r.Name, r.StartDate, r.CompanyId != id))
                .ToListAsync();

            return new CompanyDetail(company.Id, company.Name, company.CompanyTypeId, company.CompanyScaleId, company.CompanyWorkLineId,
                company.InstitutionKind, company.ServiceBranchId, cases, await CompanyFacts.OfAsync(db, id), await JobLinksAsync(id));
        }

        private async Task<List<CompanyJobLink>> JobLinksAsync(int id)
        {
            var rows = await db.Resistance
                .Where(r => !r.Deleted && r.CategoryId == KnownListIds.WorkplaceCategory && r.MainCompanyId != null && r.CompanyId != null
                    && (r.CompanyId == id || r.MainCompanyId == id)
                    && (r.EmployerRole == EmployerRole.Subcontractor || r.EmployerRole == EmployerRole.Contractor || r.EmployerRole == EmployerRole.Operator))
                .Select(r => new
                {
                    r.EmployerRole,
                    AsEmployer = r.CompanyId == id,
                    OtherId = r.CompanyId == id ? r.MainCompanyId.Value : r.CompanyId.Value,
                    OtherName = r.CompanyId == id ? r.MainCompany.Name : r.Company.Name,
                    First = r.Protestos.Where(p => !p.Deleted).Min(p => (DateTime?)p.StartDate),
                    Last = r.Protestos.Where(p => !p.Deleted).Max(p => (DateTime?)p.StartDate),
                })
                .ToListAsync();
            return rows
                .GroupBy(r => (r.EmployerRole, r.AsEmployer, r.OtherId))
                .Select(g => new CompanyJobLink(g.Key.EmployerRole, g.Key.AsEmployer, g.Key.OtherId, g.First().OtherName, g.Count(),
                    g.Min(r => r.First), g.Max(r => r.Last)))
                .OrderByDescending(l => l.Last).ThenBy(l => l.OtherName)
                .ToList();
        }
    }

    /// <summary>The line under İşveren in the case form (R37): the company's facts with other companies.</summary>
    public class GetCompanyFactsHandler(RSDBContext db)
    {
        public Task<List<CompanyRelationItem>> Handle(int companyId) => CompanyFacts.OfAsync(db, companyId);
    }

    public static class CompanyFacts
    {
        /// <summary>The company's facts with other (undeleted) companies, both directions, newest first.</summary>
        public static async Task<List<CompanyRelationItem>> OfAsync(RSDBContext db, int id) =>
            (await db.CompanyRelation
                .Where(r => (r.CompanyId == id && !r.RelatedCompany.Deleted) || (r.RelatedCompanyId == id && !r.Company.Deleted))
                .Select(r => new CompanyRelationItem(
                    r.Id, r.Kind, r.CompanyId == id,
                    r.CompanyId == id ? r.RelatedCompanyId : r.CompanyId,
                    r.CompanyId == id ? r.RelatedCompany.Name : r.Company.Name,
                    r.SharePercent, r.StartDate, r.EndDate, r.SourceUrl, r.Note))
                .ToListAsync())
            .OrderBy(r => r.Kind).ThenByDescending(r => r.StartDate).ThenBy(r => r.OtherName)
            .ToList();

        /// <summary>“Eti Bakır → Cengiz Holding (grubun parçası)”, for the activity log.</summary>
        public static string Summary(string company, string related, CompanyRelationKind kind) =>
            $"{ActivityLog.Quoted(company)} → {ActivityLog.Quoted(related)} ({Label(kind)})";

        public static string Label(CompanyRelationKind kind) => kind switch
        {
            CompanyRelationKind.Group => "grubun parçası",
            CompanyRelationKind.PublicBody => "kamu kurumuna bağlı",
            _ => "ad değiştirdi",
        };
    }

    public enum RelationStatus
    {
        Ok,
        NotFound,

        /// <summary>Error says what is wrong (the same company twice, dates in the wrong order, a share outside 0–100).</summary>
        Invalid,
    }

    public record RelationResult(RelationStatus Status, int? Id = null, string Error = null);

    /// <summary>
    /// Adds (RelationId null) or changes a fact. ThisIsFirst: the page's company is the first one (“this → other”); false
    /// for the reverse, e.g. adding a group company on the holding's page. A change keeps the two companies and the kind.
    /// </summary>
    public record SaveCompanyRelationCommand(
        int CompanyId, int? RelationId, CompanyRelationKind Kind, int OtherCompanyId, bool ThisIsFirst,
        decimal? SharePercent, DateTime? StartDate, DateTime? EndDate, string SourceUrl, string Note, string UserName);

    public class SaveCompanyRelationHandler(RSDBContext db)
    {
        public async Task<RelationResult> Handle(SaveCompanyRelationCommand command)
        {
            if (!Enum.IsDefined(command.Kind))
                return new RelationResult(RelationStatus.Invalid, Error: "Bilinmeyen ilişki türü.");
            if (command.StartDate is { } from && command.EndDate is { } to && to < from)
                return new RelationResult(RelationStatus.Invalid, Error: "Bitiş tarihi başlangıçtan önce olamaz.");
            if (command.SharePercent is < 0 or > 100)
                return new RelationResult(RelationStatus.Invalid, Error: "Pay 0 ile 100 arasında olmalı.");

            CompanyRelation relation;
            if (command.RelationId is { } relationId)
            {
                relation = await db.CompanyRelation.FirstOrDefaultAsync(r => r.Id == relationId
                    && (r.CompanyId == command.CompanyId || r.RelatedCompanyId == command.CompanyId));
                if (relation == null)
                    return new RelationResult(RelationStatus.NotFound);
            }
            else
            {
                if (command.OtherCompanyId == command.CompanyId)
                    return new RelationResult(RelationStatus.Invalid, Error: "Bir şirket kendisiyle ilişkilendirilemez.");
                int[] ids = [command.CompanyId, command.OtherCompanyId];
                if (await db.Company.CountAsync(c => ids.Contains(c.Id) && !c.Deleted) != 2)
                    return new RelationResult(RelationStatus.NotFound);
                relation = new CompanyRelation
                {
                    CompanyId = command.ThisIsFirst ? command.CompanyId : command.OtherCompanyId,
                    RelatedCompanyId = command.ThisIsFirst ? command.OtherCompanyId : command.CompanyId,
                    Kind = command.Kind,
                    CreatedBy = command.UserName,
                    CreatedAt = DateTime.UtcNow,
                };
                db.CompanyRelation.Add(relation);
            }
            relation.SharePercent = command.Kind == CompanyRelationKind.Group ? command.SharePercent : null;
            relation.StartDate = command.StartDate?.Date;
            relation.EndDate = command.EndDate?.Date;
            relation.SourceUrl = Clean(command.SourceUrl, 500);
            relation.Note = Clean(command.Note, 500);

            var names = await db.Company.Where(c => c.Id == relation.CompanyId || c.Id == relation.RelatedCompanyId)
                .Select(c => new { c.Id, c.Name }).ToListAsync();
            db.LogActivity(command.RelationId == null ? Actions.CompanyRelationAdd : Actions.CompanyRelationChange,
                CompanyFacts.Summary(names.First(n => n.Id == relation.CompanyId).Name, names.First(n => n.Id == relation.RelatedCompanyId).Name, relation.Kind),
                subject: "companies", subjectId: command.CompanyId);
            await db.SaveChangesAsync();
            return new RelationResult(RelationStatus.Ok, relation.Id);
        }

        private static string Clean(string text, int max) =>
            string.IsNullOrWhiteSpace(text) ? null : text.Trim() is { } t && t.Length > max ? t[..max] : text.Trim();
    }

    public record RemoveCompanyRelationCommand(int CompanyId, int RelationId);

    public class RemoveCompanyRelationHandler(RSDBContext db)
    {
        public async Task<RelationResult> Handle(RemoveCompanyRelationCommand command)
        {
            var relation = await db.CompanyRelation
                .Include(r => r.Company).Include(r => r.RelatedCompany)
                .FirstOrDefaultAsync(r => r.Id == command.RelationId && (r.CompanyId == command.CompanyId || r.RelatedCompanyId == command.CompanyId));
            if (relation == null)
                return new RelationResult(RelationStatus.NotFound);
            db.CompanyRelation.Remove(relation);
            db.LogActivity(Actions.CompanyRelationRemove, CompanyFacts.Summary(relation.Company.Name, relation.RelatedCompany.Name, relation.Kind),
                subject: "companies", subjectId: command.CompanyId);
            await db.SaveChangesAsync();
            return new RelationResult(RelationStatus.Ok);
        }
    }
}
