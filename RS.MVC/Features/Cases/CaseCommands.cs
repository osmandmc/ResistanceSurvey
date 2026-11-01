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
using RS.MVC.Models;
using static RS.COMMON.Constants.Enums;

namespace RS.MVC.Features.Cases
{
    public enum CaseCommandStatus
    {
        Ok,
        Invalid,
        NotFound,
    }

    public record CaseCommandResult(CaseCommandStatus Status, int? Id = null, IReadOnlyDictionary<string, string> Errors = null)
    {
        public static readonly CaseCommandResult NotFound = new(CaseCommandStatus.NotFound);

        public static CaseCommandResult Invalid(IReadOnlyDictionary<string, string> errors) => new(CaseCommandStatus.Invalid, Errors: errors);

        public static CaseCommandResult Ok(int id) => new(CaseCommandStatus.Ok, id);
    }

    /// <summary>
    /// The one save path for cases and protests (F2): creating a case (with or without an article), editing it, adding and
    /// editing protests. Replaces ResistanceApplication's Create and AddProtesto.
    /// </summary>
    public class CaseWriter(RSDBContext db)
    {
        /// <summary>
        /// A new case with its first protest, saved. Start and end come from that protest (as before); the article's News
        /// row, when there is one, is linked. <paramref name="reasonIds"/> and <paramref name="corporationIds"/>: the case's
        /// lists after typed values were resolved (ListValueResolver).
        /// </summary>
        public async Task<int> CreateCaseAsync(
            CaseInput c, ProtestInput p, string userName, IReadOnlyList<int> reasonIds, IReadOnlyList<int> corporationIds, int? newsId = null)
        {
            var resistance = new Resistance { Creator = userName, CreateDate = DateTime.Now, StartDate = p.StartDate!.Value, EndDate = p.Ongoing ? null : p.EndDate };
            await ApplyCaseAsync(resistance, c, reasonIds, corporationIds);
            var protest = new Protesto { Creator = userName, CreateDate = DateTime.Now };
            await ApplyProtestAsync(protest, p);
            resistance.Protestos = [protest];
            resistance.ResistanceNews = newsId is { } id ? [new ResistanceNews { NewsId = id }] : [];
            db.Resistance.Add(resistance);
            await db.SaveChangesAsync();
            return resistance.Id;
        }

        /// <summary>A new protest on the case, saved; the case's start date follows its earliest protest.</summary>
        public async Task<int> AddProtestAsync(Resistance resistance, ProtestInput p, string userName)
        {
            var protest = new Protesto { ResistanceId = resistance.Id, Creator = userName, CreateDate = DateTime.Now };
            await ApplyProtestAsync(protest, p);
            db.Protesto.Add(protest);
            await db.SaveChangesAsync();
            await UpdateStartDateAsync(resistance);
            await db.SaveChangesAsync();
            return protest.Id;
        }

        /// <summary>
        /// Copies the form's case part onto the entity and replaces its reasons, organizations and employment types (R31:
        /// by category). İşletme: the workplace, its main company, union and counts, the result; organizations support.
        /// Genel and Dayanışma: organizations organize; Dayanışma may name the workplace whose workers acted and the
        /// supported case. "Kime karşı" and the target institution are no longer set.
        /// </summary>
        public async Task ApplyCaseAsync(Resistance r, CaseInput c, IReadOnlyList<int> reasonIds, IReadOnlyList<int> corporationIds)
        {
            var workplace = c.CategoryId == KnownListIds.WorkplaceCategory;
            var solidarity = c.CategoryId == KnownListIds.SolidarityCategory;
            r.Name = c.Name;
            r.Description = c.Description;
            r.Note = string.IsNullOrWhiteSpace(c.Note) ? null : c.Note.Trim();
            r.CategoryId = c.CategoryId!.Value;
            // R37: the employer, how it works in this case, the organization above it and the place (İşletme only).
            var employerRole = workplace ? EmployerRoles.Effective(c) : EmployerRole.Unknown;
            r.EmployerUnknown = workplace && c.EmployerUnknown;
            r.CompanyId = (workplace && !c.EmployerUnknown) || solidarity ? c.CompanyId : null;
            r.EmployerRole = employerRole;
            r.MainCompanyId = workplace && EmployerRoles.HasAbove(employerRole) ? c.MainCompanyId : null;
            r.WorkplaceId = workplace && c.WorkplaceId is { } workplaceId && await db.Workplace.AnyAsync(w => w.Id == workplaceId && !w.Deleted)
                ? workplaceId
                : null;
            r.SupportedResistanceId = solidarity ? c.SupportedCaseId : null;
            r.DevelopRight = c.DevelopRight!.Value;
            r.EmployeeCountNumber = workplace ? c.EmployeeCount : null;
            r.EmployeeCountId = workplace ? c.EmployeeCountRangeId : null;
            r.TradeUnionAuthorityId = workplace ? c.TradeUnionAuthorityId : null;
            r.TradeUnionId = workplace ? c.TradeUnionId : null;
            r.FiredEmployeeCountByProtesto = workplace ? c.FiredEmployeeCount ?? 0 : 0;
            r.ResistanceResult = workplace ? c.Result!.Value : ResistanceResult.Unknown;
            var role = workplace ? OrganizationRole.Supporting : OrganizationRole.Organizing;
            var employmentTypeIds = workplace ? c.EmploymentTypeIds : [];

            if (r.Id != 0)
            {
                db.ResistanceResistanceReason.RemoveRange(await db.ResistanceResistanceReason.Where(x => x.ResistanceId == r.Id).ToListAsync());
                db.ResistanceCorporation.RemoveRange(await db.ResistanceCorporation.Where(x => x.ResistanceId == r.Id).ToListAsync());
                db.ResistanceEmploymentType.RemoveRange(await db.ResistanceEmploymentType.Where(x => x.ResistanceId == r.Id).ToListAsync());
            }
            r.ResistanceResistanceReasons = reasonIds.Distinct().Select(id => new ResistanceResistanceReason { ResistanceReasonId = id }).ToList();
            r.ResistanceCorporations = corporationIds.Distinct().Select(id => new ResistanceCorporation { CorporationId = id, Role = role }).ToList();
            r.ResistanceEmploymentTypes = employmentTypeIds.Distinct().Select(id => new ResistanceEmploymentType { EmploymentTypeId = id }).ToList();
            await EnsureOutsourceRelationAsync(r.CompanyId, r.MainCompanyId);
        }

        /// <summary>Why the supported case cannot be used (R31), or null: it must exist, be live and be another case.</summary>
        public async Task<string> SupportedCaseProblemAsync(CaseInput c, int? selfId = null)
        {
            if (c?.CategoryId != KnownListIds.SolidarityCategory || c.SupportedCaseId is not { } supported)
                return null;
            if (supported == selfId)
                return "Bir vaka kendisini destekleyemez.";
            return await db.Resistance.AnyAsync(x => x.Id == supported && !x.Deleted) ? null : "Desteklenen vaka bulunamadı.";
        }

        /// <summary>The case's start date follows its earliest protest (as in the old code).</summary>
        public async Task UpdateStartDateAsync(Resistance resistance)
        {
            var earliest = await db.Protesto
                .Where(p => p.ResistanceId == resistance.Id && !p.Deleted)
                .Select(p => (DateTime?)p.StartDate)
                .MinAsync();
            resistance.StartDate = earliest ?? default;
        }

        /// <summary>Copies the form's protest onto the entity and replaces its types, places, interventions and locations.</summary>
        public async Task ApplyProtestAsync(Protesto protest, ProtestInput input)
        {
            protest.StartDate = input.StartDate!.Value;
            protest.EndDate = input.Ongoing ? null : input.EndDate;
            protest.GenderId = input.GenderId!.Value;
            protest.CustodyCount = input.CustodyCount;
            protest.EmployeeCountNumber = input.EmployeeCount;
            protest.ProtestoEmployeeCountId = input.EmployeeCountRangeId;
            protest.SimpleProtestoDescription = input.SimpleDescription;
            protest.StrikeDuration = input.StrikeDurationSeconds ?? 0;
            protest.Note = input.Note;

            if (protest.Id != 0)
            {
                db.ProtestoProtestoType.RemoveRange(await db.ProtestoProtestoType.Where(x => x.ProtestoId == protest.Id).ToListAsync());
                db.ProtestoProtestoPlace.RemoveRange(await db.ProtestoProtestoPlace.Where(x => x.ProtestoId == protest.Id).ToListAsync());
                db.ProtestoInterventionType.RemoveRange(await db.ProtestoInterventionType.Where(x => x.ProtestoId == protest.Id).ToListAsync());
                db.ProtestoLocation.RemoveRange(await db.ProtestoLocation.Where(x => x.ProtestoId == protest.Id).ToListAsync());
            }
            protest.ProtestoProtestoTypes = input.ProtestTypeIds.Distinct().Select(id => new ProtestoProtestoType { ProtestoTypeId = id }).ToList();
            protest.ProtestoProtestoPlaces = input.PlaceIds.Distinct().Select(id => new ProtestoProtestoPlace { ProtestoPlaceId = id }).ToList();
            protest.ProtestoInterventionTypes = input.InterventionTypeIds.Distinct().Select(id => new ProtestoInterventionType { InterventionTypeId = id }).ToList();
            protest.Locations = (input.Locations ?? [])
                .Select(l => new ProtestoLocation { CityId = l.CityId!.Value, DistrictId = l.DistrictId, Place = l.Place })
                .ToList();
        }

        /// <summary>Records "subcontractor of" for the companies (creating a case did this; editing now does too).</summary>
        public async Task EnsureOutsourceRelationAsync(int? companyId, int? mainCompanyId)
        {
            if (companyId is not { } outsource || mainCompanyId is not { } main)
                return;
            if (!await db.CompanyOutsourceCompany.AnyAsync(c => c.CompanyId == main && c.OutsourceCompanyId == outsource))
                db.CompanyOutsourceCompany.Add(new CompanyOutsourceCompany { CompanyId = main, OutsourceCompanyId = outsource });
        }
    }

    /// <summary>Edit the case part (not its protests or articles).</summary>
    public record UpdateCaseCommand(int Id, CaseInput Case, string UserName);

    public class UpdateCaseHandler(RSDBContext db, CaseWriter writer, ListValueResolver lists)
    {
        public async Task<CaseCommandResult> Handle(UpdateCaseCommand command)
        {
            var errors = ReviewValidation.ForCase(command.Case);
            if (await writer.SupportedCaseProblemAsync(command.Case, command.Id) is { } supported)
                errors["case.supportedCaseId"] = supported;
            if (errors.Count > 0)
                return CaseCommandResult.Invalid(errors);

            var r = await db.Resistance.FirstOrDefaultAsync(x => x.Id == command.Id && !x.Deleted);
            if (r == null)
                return CaseCommandResult.NotFound;

            await using var transaction = await db.Database.BeginTransactionAsync();
            var c = command.Case;
            var reasonIds = await lists.ReasonIdsAsync(c.ReasonIds, c.NewReasonNames);
            var corporationIds = await lists.CorporationIdsAsync(c.CorporationIds, c.NewCorporationNames);
            await writer.ApplyCaseAsync(r, c, reasonIds, corporationIds);
            r.Updater = command.UserName;
            r.UpdateDate = DateTime.Now;
            db.LogActivity(Actions.CaseUpdate, r.Name, r.Id);

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return CaseCommandResult.Ok(r.Id);
        }
    }

    /// <summary>Add a protest to the case (ProtestId null) or edit one of its protests.</summary>
    public record SaveProtestCommand(int CaseId, int? ProtestId, ProtestInput Protest, string UserName);

    public class SaveProtestHandler(RSDBContext db, CaseWriter writer)
    {
        public async Task<CaseCommandResult> Handle(SaveProtestCommand command)
        {
            var errors = ReviewValidation.ForProtest(command.Protest);
            if (errors.Count > 0)
                return CaseCommandResult.Invalid(errors);

            var resistance = await db.Resistance.FirstOrDefaultAsync(x => x.Id == command.CaseId && !x.Deleted);
            if (resistance == null)
                return CaseCommandResult.NotFound;

            Protesto protest;
            if (command.ProtestId is { } protestId)
            {
                protest = await db.Protesto.FirstOrDefaultAsync(p => p.Id == protestId && p.ResistanceId == command.CaseId && !p.Deleted);
                if (protest == null)
                    return CaseCommandResult.NotFound;
                protest.Updater = command.UserName;
                protest.UpdateDate = DateTime.Now;
            }
            else
            {
                protest = new Protesto { ResistanceId = command.CaseId, Creator = command.UserName, CreateDate = DateTime.Now };
                db.Protesto.Add(protest);
            }

            await using var transaction = await db.Database.BeginTransactionAsync();
            await writer.ApplyProtestAsync(protest, command.Protest);
            await db.SaveChangesAsync();
            await writer.UpdateStartDateAsync(resistance);
            db.LogActivity(command.ProtestId == null ? Actions.ProtestAdd : Actions.ProtestUpdate,
                $"{ActivityLog.Day(protest.StartDate)} eylemi", command.CaseId, subject: "protest", subjectId: protest.Id);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return CaseCommandResult.Ok(protest.Id);
        }
    }

    public record DeleteProtestCommand(int CaseId, int ProtestId, string UserName);

    /// <summary>Marks the protest deleted (as before; nothing is removed from the database).</summary>
    public class DeleteProtestHandler(RSDBContext db, CaseWriter writer)
    {
        public async Task<CaseCommandResult> Handle(DeleteProtestCommand command)
        {
            var protest = await db.Protesto.FirstOrDefaultAsync(p => p.Id == command.ProtestId && p.ResistanceId == command.CaseId && !p.Deleted);
            var resistance = await db.Resistance.FirstOrDefaultAsync(x => x.Id == command.CaseId && !x.Deleted);
            if (protest == null || resistance == null)
                return CaseCommandResult.NotFound;

            protest.Deleted = true;
            protest.Updater = command.UserName;
            protest.UpdateDate = DateTime.Now;
            db.LogActivity(Actions.ProtestDelete, $"{ActivityLog.Day(protest.StartDate)} eylemi", command.CaseId,
                subject: "protest", subjectId: protest.Id);
            await db.SaveChangesAsync();
            await writer.UpdateStartDateAsync(resistance);
            await db.SaveChangesAsync();
            return CaseCommandResult.Ok(protest.Id);
        }
    }

    public record DeleteCaseCommand(int Id, string UserName);

    /// <summary>Marks the case deleted (as before). Its articles stay handled; their links remain for the record.</summary>
    public class DeleteCaseHandler(RSDBContext db)
    {
        public async Task<CaseCommandResult> Handle(DeleteCaseCommand command)
        {
            var r = await db.Resistance.FirstOrDefaultAsync(x => x.Id == command.Id && !x.Deleted);
            if (r == null)
                return CaseCommandResult.NotFound;
            r.Deleted = true;
            r.Updater = command.UserName;
            r.UpdateDate = DateTime.Now;
            db.LogActivity(Actions.CaseDelete, r.Name, r.Id);
            await db.SaveChangesAsync();
            return CaseCommandResult.Ok(r.Id);
        }
    }
}
