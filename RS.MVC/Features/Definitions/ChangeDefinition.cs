using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;
using RS.MVC.Features.Activity;

namespace RS.MVC.Features.Definitions
{
    /// <summary>The value's name and its kind's own fields (the screen sends the whole row; fields of other kinds are ignored).</summary>
    public record UpdateDefinitionCommand(
        DefinitionKind Kind,
        int Id,
        string Name,
        int? TypeId = null,
        bool AgainstProduction = false,
        bool Simple = false,
        int? ScaleId = null,
        int? WorklineId = null,
        int? ConfederationId = null,
        RS.COMMON.Entities.InstitutionKind? InstitutionKind = null,
        int? ServiceBranchId = null,
        int? CityId = null,
        int? DistrictId = null,
        int? OwnerCompanyId = null);

    public enum ChangeStatus
    {
        Ok,
        NotFound,

        /// <summary>Empty name.</summary>
        Invalid,

        /// <summary>Another value already has this name: merge instead (ExistingId).</summary>
        Duplicate,

        /// <summary>Still used (Usage live cases or protests, maybe 0 when only deleted ones use it): merge instead.</summary>
        InUse,
    }

    public record ChangeResult(ChangeStatus Status, int? ExistingId = null, string ExistingName = null, int Usage = 0)
    {
        public static readonly ChangeResult Ok = new(ChangeStatus.Ok);
        public static readonly ChangeResult NotFound = new(ChangeStatus.NotFound);
    }

    public class UpdateDefinitionHandler(RSDBContext db)
    {
        public async Task<ChangeResult> Handle(UpdateDefinitionCommand command)
        {
            var name = command.Name?.Trim();
            if (string.IsNullOrEmpty(name))
                return new ChangeResult(ChangeStatus.Invalid);
            var oldName = await DefinitionKinds.NameAsync(db, command.Kind, command.Id);
            (int Id, COMMON.Entities.LookupEntity.OrganizationKind Kind, int? Confederation)? changedKind = null;

            if (command.Kind == DefinitionKind.Companies)
            {
                var company = await db.Company.FirstOrDefaultAsync(c => c.Id == command.Id && !c.Deleted);
                if (company == null)
                    return ChangeResult.NotFound;
                var same = await db.Company.Where(c => c.Id != command.Id && !c.Deleted && c.Name == name).Select(c => new { c.Id, c.Name }).FirstOrDefaultAsync();
                if (same != null)
                    return new ChangeResult(ChangeStatus.Duplicate, same.Id, same.Name);
                company.Name = name;
                company.CompanyTypeId = command.TypeId;
                company.CompanyScaleId = command.ScaleId;
                company.CompanyWorkLineId = command.WorklineId;
                company.InstitutionKind = command.InstitutionKind;
                company.ServiceBranchId = command.ServiceBranchId;
            }
            else if (command.Kind == DefinitionKind.Workplaces)
            {
                // R37: a place's name, province, district and whom it belongs to. The same name in another province is
                // another place; a district must lie in the province; the owner must be a company that is not deleted.
                var workplace = await db.Workplace.FirstOrDefaultAsync(w => w.Id == command.Id && !w.Deleted);
                if (workplace == null)
                    return ChangeResult.NotFound;
                var same = await db.Workplace.Where(w => w.Id != command.Id && !w.Deleted && w.Name == name && w.CityId == command.CityId)
                    .Select(w => new { w.Id, w.Name }).FirstOrDefaultAsync();
                if (same != null)
                    return new ChangeResult(ChangeStatus.Duplicate, same.Id, same.Name);
                workplace.Name = name;
                workplace.CityId = command.CityId is { } cityId && await db.City.AnyAsync(c => c.Id == cityId) ? cityId : null;
                workplace.DistrictId = command.DistrictId is { } districtId && await db.District.AnyAsync(d => d.Id == districtId && d.CityId == workplace.CityId)
                    ? districtId : null;
                workplace.OwnerCompanyId = command.OwnerCompanyId is { } ownerId && await db.Company.AnyAsync(c => c.Id == ownerId && !c.Deleted)
                    ? ownerId : null;
            }
            else
            {
                var value = await DefinitionValues.FindAsync(db, command.Kind, command.Id);
                if (value == null)
                    return ChangeResult.NotFound;
                var same = await DefinitionValues.SameNameAsync(db, command.Kind, command.Id, name);
                if (same != null)
                    return new ChangeResult(ChangeStatus.Duplicate, same.Id, same.Name);
                value.Name = name;
                switch (value)
                {
                    case Corporation corporation:
                        corporation.CorporationTypeId = command.TypeId;
                        var confederation = await Organizations.ConfederationAsync(db, command.ConfederationId);
                        if (confederation == corporation.Id)
                            confederation = null;
                        var kind = Organizations.KindOf(command.TypeId);
                        if (kind != Organizations.KindOf(corporation))
                            changedKind = (corporation.Id, kind, confederation); // after the save (R34)
                        else if (corporation is TradeUnion union)
                            union.ConfederationId = confederation;
                        break;
                    case ProtestoType protestType:
                        protestType.AgainstProduction = command.AgainstProduction;
                        protestType.Simple = command.Simple;
                        break;
                }
            }
            var summary = oldName != name ? $"{DefinitionKinds.Label(command.Kind)} {ActivityLog.Quoted(oldName)} → {ActivityLog.Quoted(name)}"
                : $"{DefinitionKinds.Label(command.Kind)} {ActivityLog.Quoted(name)}";
            db.LogActivity(Actions.DefinitionUpdate, summary, subject: DefinitionKinds.Slug(command.Kind), subjectId: command.Id);
            await db.SaveChangesAsync();
            if (changedKind is { } k)
                await Organizations.SetKindAsync(db, k.Id, k.Kind, k.Confederation);
            return ChangeResult.Ok;
        }
    }

    public record ApproveDefinitionCommand(DefinitionKind Kind, int Id, bool Approved = true);

    public class ApproveDefinitionHandler(RSDBContext db)
    {
        public async Task<ChangeResult> Handle(ApproveDefinitionCommand command)
        {
            if (!DefinitionKinds.HasApproval(command.Kind))
                return ChangeResult.NotFound;
            var value = await DefinitionValues.FindAsync(db, command.Kind, command.Id);
            if (value == null)
                return ChangeResult.NotFound;
            value.Approved = command.Approved;
            db.LogActivity(command.Approved ? Actions.DefinitionApprove : Actions.DefinitionUnapprove,
                $"{DefinitionKinds.Label(command.Kind)} {ActivityLog.Quoted(value.Name)}", subject: DefinitionKinds.Slug(command.Kind), subjectId: command.Id);
            await db.SaveChangesAsync();
            return ChangeResult.Ok;
        }
    }

    public record DeleteDefinitionCommand(DefinitionKind Kind, int Id);

    /// <summary>
    /// Only unused values go. Companies are marked deleted (old cases may still point to them) and lose their
    /// subcontractor links; other values are removed, so not even a deleted case may use them. Otherwise: merge.
    /// </summary>
    public class DeleteDefinitionHandler(RSDBContext db)
    {
        public async Task<ChangeResult> Handle(DeleteDefinitionCommand command)
        {
            var usage = (await DefinitionUsage.CountAsync(db, command.Kind, [command.Id])).GetValueOrDefault(command.Id);
            var name = await DefinitionKinds.NameAsync(db, command.Kind, command.Id);
            if (command.Kind == DefinitionKind.Companies)
            {
                var company = await db.Company.FirstOrDefaultAsync(c => c.Id == command.Id && !c.Deleted);
                if (company == null)
                    return ChangeResult.NotFound;
                if (usage > 0)
                    return new ChangeResult(ChangeStatus.InUse, Usage: usage);
                company.Deleted = true;
                db.CompanyOutsourceCompany.RemoveRange(
                    db.CompanyOutsourceCompany.Where(l => l.CompanyId == command.Id || l.OutsourceCompanyId == command.Id));
            }
            else
            {
                var value = await DefinitionValues.FindAsync(db, command.Kind, command.Id);
                if (value == null)
                    return ChangeResult.NotFound;
                if (usage > 0 || await DefinitionValues.AnyReferenceAsync(db, command.Kind, command.Id))
                    return new ChangeResult(ChangeStatus.InUse, Usage: usage);
                db.Remove(value);
            }
            db.LogActivity(Actions.DefinitionDelete,
                $"{DefinitionKinds.Label(command.Kind)} {ActivityLog.Quoted(name)}",
                subject: DefinitionKinds.Slug(command.Kind), subjectId: command.Id);
            await db.SaveChangesAsync();
            return ChangeResult.Ok;
        }
    }

    /// <summary>Access to the approvable lists by kind.</summary>
    internal static class DefinitionValues
    {
        public static async Task<IApprovable> FindAsync(RSDBContext db, DefinitionKind kind, int id) => kind switch
        {
            DefinitionKind.Corporations => await db.Corporation.FindAsync(id),
            DefinitionKind.Reasons => await db.ResistanceReason.FindAsync(id),
            DefinitionKind.ProtestTypes => await db.ProtestoType.FindAsync(id),
            DefinitionKind.ProtestPlaces => await db.ProtestoPlace.FindAsync(id),
            DefinitionKind.Workplaces => await db.Workplace.FirstOrDefaultAsync(w => w.Id == id && !w.Deleted),
            _ => null,
        };

        /// <summary>Another value of the list with this name (the database compares by Turkish rules, F9).</summary>
        public static async Task<LookupEntity> SameNameAsync(RSDBContext db, DefinitionKind kind, int id, string name)
        {
            IQueryable<LookupEntity> values = kind switch
            {
                DefinitionKind.Corporations => db.Corporation,
                DefinitionKind.Reasons => db.ResistanceReason,
                DefinitionKind.ProtestTypes => db.ProtestoType,
                DefinitionKind.Workplaces => db.Workplace.Where(w => !w.Deleted),
                _ => db.ProtestoPlace,
            };
            return await values.Where(v => v.Id != id && v.Name == name).FirstOrDefaultAsync();
        }

        /// <summary>Any row pointing at the value, deleted cases and protests included (a delete would break them).</summary>
        public static async Task<bool> AnyReferenceAsync(RSDBContext db, DefinitionKind kind, int id) => kind switch
        {
            DefinitionKind.Corporations => await db.ResistanceCorporation.AnyAsync(x => x.CorporationId == id)
                || await db.Resistance.AnyAsync(r => r.TradeUnionId == id)
                || await db.Set<TradeUnion>().AnyAsync(u => u.ConfederationId == id), // a confederation with unions
            DefinitionKind.Reasons => await db.ResistanceResistanceReason.AnyAsync(x => x.ResistanceReasonId == id),
            DefinitionKind.ProtestTypes => await db.ProtestoProtestoType.AnyAsync(x => x.ProtestoTypeId == id),
            DefinitionKind.ProtestPlaces => await db.ProtestoProtestoPlace.AnyAsync(x => x.ProtestoPlaceId == id),
            DefinitionKind.Workplaces => await db.Resistance.AnyAsync(r => r.WorkplaceId == id),
            _ => false,
        };
    }
}
