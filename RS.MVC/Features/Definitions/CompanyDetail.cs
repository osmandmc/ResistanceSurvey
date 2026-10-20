using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.COMMON.Entities;
using RS.EF;
using RS.MVC.Features.Review;

namespace RS.MVC.Features.Definitions
{
    /// <summary>A case using the company. AsMainCompany: the company is the case's main company (the case is against its subcontractor).</summary>
    public record CompanyCase(int Id, string Name, DateTime StartDate, bool AsMainCompany);

    /// <summary>
    /// One company in Tanımlar (R12a-3): its fields, its live cases, and its subcontractor relations
    /// (MainCompanies: it works for them as a subcontractor; Subcontractors: they work for it).
    /// </summary>
    public record CompanyDetail(
        int Id,
        string Name,
        int? TypeId,
        int? ScaleId,
        int? WorklineId,
        IReadOnlyList<CompanyCase> Cases,
        IReadOnlyList<Option> MainCompanies,
        IReadOnlyList<Option> Subcontractors);

    public class GetCompanyHandler(RSDBContext db)
    {
        /// <summary>Null when there is no such (undeleted) company.</summary>
        public async Task<CompanyDetail> Handle(int id)
        {
            var company = await db.Company.Where(c => c.Id == id && !c.Deleted)
                .Select(c => new { c.Id, c.Name, c.CompanyTypeId, c.CompanyScaleId, c.CompanyWorkLineId })
                .FirstOrDefaultAsync();
            if (company == null)
                return null;

            var cases = await db.Resistance
                .Where(r => !r.Deleted && (r.CompanyId == id || r.MainCompanyId == id))
                .OrderByDescending(r => r.StartDate).ThenByDescending(r => r.Id)
                .Select(r => new CompanyCase(r.Id, r.Name, r.StartDate, r.CompanyId != id))
                .ToListAsync();
            var mainCompanies = await db.CompanyOutsourceCompany
                .Where(l => l.OutsourceCompanyId == id && !l.Company.Deleted)
                .OrderBy(l => l.Company.Name)
                .Select(l => new Option(l.CompanyId, l.Company.Name))
                .ToListAsync();
            var subcontractors = await db.CompanyOutsourceCompany
                .Where(l => l.CompanyId == id && !l.OutsourceCompany.Deleted)
                .OrderBy(l => l.OutsourceCompany.Name)
                .Select(l => new Option(l.OutsourceCompanyId, l.OutsourceCompany.Name))
                .ToListAsync();

            return new CompanyDetail(company.Id, company.Name, company.CompanyTypeId, company.CompanyScaleId, company.CompanyWorkLineId,
                cases, mainCompanies.DistinctBy(o => o.Id).ToList(), subcontractors.DistinctBy(o => o.Id).ToList());
        }
    }

    public enum LinkStatus
    {
        Ok,
        NotFound,

        /// <summary>A company cannot be its own subcontractor.</summary>
        SameCompany,
    }

    /// <summary>"Subcontractor works for MainCompany". Adding an existing link changes nothing.</summary>
    public record CompanyLinkCommand(int MainCompanyId, int SubcontractorId);

    public class AddCompanyLinkHandler(RSDBContext db)
    {
        public async Task<LinkStatus> Handle(CompanyLinkCommand command)
        {
            if (command.MainCompanyId == command.SubcontractorId)
                return LinkStatus.SameCompany;
            int[] ids = [command.MainCompanyId, command.SubcontractorId];
            if (await db.Company.CountAsync(c => ids.Contains(c.Id) && !c.Deleted) != 2)
                return LinkStatus.NotFound;
            if (!await db.CompanyOutsourceCompany.AnyAsync(l => l.CompanyId == command.MainCompanyId && l.OutsourceCompanyId == command.SubcontractorId))
            {
                db.CompanyOutsourceCompany.Add(new CompanyOutsourceCompany { CompanyId = command.MainCompanyId, OutsourceCompanyId = command.SubcontractorId });
                await db.SaveChangesAsync();
            }
            return LinkStatus.Ok;
        }
    }

    /// <summary>
    /// Removes the relation. Cases keep their main company: a case records the relation as it was at the time,
    /// so ending it does not rewrite past cases.
    /// </summary>
    public class RemoveCompanyLinkHandler(RSDBContext db)
    {
        public async Task<LinkStatus> Handle(CompanyLinkCommand command)
        {
            var links = await db.CompanyOutsourceCompany
                .Where(l => l.CompanyId == command.MainCompanyId && l.OutsourceCompanyId == command.SubcontractorId)
                .ToListAsync();
            if (links.Count == 0)
                return LinkStatus.NotFound;
            db.CompanyOutsourceCompany.RemoveRange(links);
            await db.SaveChangesAsync();
            return LinkStatus.Ok;
        }
    }
}
