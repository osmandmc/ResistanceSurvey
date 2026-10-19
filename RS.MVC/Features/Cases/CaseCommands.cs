using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.COMMON.Entities;
using RS.EF;
using RS.MVC.Features.Review;
using RS.MVC.Models;

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

    /// <summary>What the case commands share: protest fields, link tables, the case's start date.</summary>
    public class CaseWriter(RSDBContext db)
    {
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
            protest.EndDate = input.EndDate;
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
            if (errors.Count > 0)
                return CaseCommandResult.Invalid(errors);

            var r = await db.Resistance.FirstOrDefaultAsync(x => x.Id == command.Id && !x.Deleted);
            if (r == null)
                return CaseCommandResult.NotFound;

            await using var transaction = await db.Database.BeginTransactionAsync();
            var c = command.Case;
            var againstCompany = c.TargetType == ResistanceTargetType.Company;
            r.Name = c.Name;
            r.Description = c.Description;
            r.CategoryId = c.CategoryId!.Value;
            r.ResistanceTargetType = c.TargetType!.Value;
            r.CompanyId = againstCompany ? c.CompanyId : null;
            r.MainCompanyId = againstCompany && c.IsOutsource ? c.MainCompanyId : null;
            r.CorporationId = againstCompany ? null : c.TargetCorporationId;
            r.DevelopRight = c.DevelopRight!.Value;
            r.EmployeeCountNumber = c.EmployeeCount;
            r.EmployeeCountId = c.EmployeeCountRangeId;
            r.TradeUnionAuthorityId = c.TradeUnionAuthorityId;
            r.TradeUnionId = c.TradeUnionId;
            r.FiredEmployeeCountByProtesto = c.FiredEmployeeCount ?? 0;
            r.ResistanceResult = c.Result!.Value;
            r.Updater = command.UserName;
            r.UpdateDate = DateTime.Now;

            var reasonIds = await lists.ReasonIdsAsync(c.ReasonIds, c.NewReasonNames);
            var corporationIds = await lists.CorporationIdsAsync(c.CorporationIds, c.NewCorporationNames);
            db.ResistanceResistanceReason.RemoveRange(await db.ResistanceResistanceReason.Where(x => x.ResistanceId == r.Id).ToListAsync());
            db.ResistanceCorporation.RemoveRange(await db.ResistanceCorporation.Where(x => x.ResistanceId == r.Id).ToListAsync());
            db.ResistanceEmploymentType.RemoveRange(await db.ResistanceEmploymentType.Where(x => x.ResistanceId == r.Id).ToListAsync());
            db.ResistanceResistanceReason.AddRange(reasonIds.Select(id => new ResistanceResistanceReason { ResistanceId = r.Id, ResistanceReasonId = id }));
            db.ResistanceCorporation.AddRange(corporationIds.Select(id => new ResistanceCorporation { ResistanceId = r.Id, CorporationId = id }));
            db.ResistanceEmploymentType.AddRange(c.EmploymentTypeIds.Distinct().Select(id => new ResistanceEmploymentType { ResistanceId = r.Id, EmploymentTypeId = id }));
            await writer.EnsureOutsourceRelationAsync(r.CompanyId, r.MainCompanyId);

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
            await db.SaveChangesAsync();
            return CaseCommandResult.Ok(r.Id);
        }
    }
}
