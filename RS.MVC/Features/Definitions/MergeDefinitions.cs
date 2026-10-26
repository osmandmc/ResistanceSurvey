using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.EF;
using RS.MVC.Features.Activity;

namespace RS.MVC.Features.Definitions
{
    /// <summary>Merge From into Into: everything that used From uses Into, then From goes.</summary>
    public record MergeDefinitionsCommand(DefinitionKind Kind, int FromId, int IntoId);

    public enum MergeStatus
    {
        Ok,
        NotFound,

        /// <summary>From and Into are the same value.</summary>
        SameValue,
    }

    /// <summary>Affected: live cases (protests for protest types and places) that used From.</summary>
    public record MergeResult(MergeStatus Status, int Affected = 0);

    /// <summary>
    /// Moves every reference in one save (R12a), fixing the old pages' replace: those deleted the case links when the
    /// target was not used yet, let a case hold the same value twice, and for companies moved only the case's company.
    /// Here every reference moves (companies: company, main company and subcontractor links); a case that already has
    /// Into keeps one link. Companies are marked deleted; other values are removed.
    /// </summary>
    public class MergeDefinitionsHandler(RSDBContext db)
    {
        /// <summary>How many live cases (or protests) a merge of From would touch, for the confirmation.</summary>
        public async Task<int> PreviewAsync(DefinitionKind kind, int fromId) =>
            (await DefinitionUsage.CountAsync(db, kind, [fromId])).GetValueOrDefault(fromId);

        public async Task<MergeResult> Handle(MergeDefinitionsCommand command)
        {
            if (command.FromId == command.IntoId)
                return new MergeResult(MergeStatus.SameValue);
            if (!await BothExistAsync(command))
                return new MergeResult(MergeStatus.NotFound);

            var affected = await PreviewAsync(command.Kind, command.FromId);
            var fromName = await DefinitionKinds.NameAsync(db, command.Kind, command.FromId);
            var intoName = await DefinitionKinds.NameAsync(db, command.Kind, command.IntoId);
            var used = DefinitionKinds.CountsProtests(command.Kind) ? "eylem" : "vaka";
            db.LogActivity(Actions.DefinitionMerge,
                $"{DefinitionKinds.Label(command.Kind)} {ActivityLog.Quoted(fromName)} → {ActivityLog.Quoted(intoName)} ({affected} {used})",
                subject: DefinitionKinds.Slug(command.Kind), subjectId: command.IntoId);
            switch (command.Kind)
            {
                case DefinitionKind.Companies:
                    await MergeCompaniesAsync(command.FromId, command.IntoId);
                    break;
                case DefinitionKind.Corporations:
                    await MoveLinksAsync(db.ResistanceCorporation, x => x.CorporationId, x => x.ResistanceId,
                        command.FromId, command.IntoId, (x, id) => x.CorporationId = id);
                    foreach (var r in await db.Resistance.Where(r => r.CorporationId == command.FromId || r.TradeUnionId == command.FromId).ToListAsync())
                    {
                        if (r.CorporationId == command.FromId) r.CorporationId = command.IntoId;
                        if (r.TradeUnionId == command.FromId) r.TradeUnionId = command.IntoId;
                    }
                    db.Remove(await db.Corporation.FindAsync(command.FromId));
                    break;
                case DefinitionKind.Reasons:
                    await MoveLinksAsync(db.ResistanceResistanceReason, x => x.ResistanceReasonId, x => x.ResistanceId,
                        command.FromId, command.IntoId, (x, id) => x.ResistanceReasonId = id);
                    db.Remove(await db.ResistanceReason.FindAsync(command.FromId));
                    break;
                case DefinitionKind.ProtestTypes:
                    await MoveLinksAsync(db.ProtestoProtestoType, x => x.ProtestoTypeId, x => x.ProtestoId,
                        command.FromId, command.IntoId, (x, id) => x.ProtestoTypeId = id);
                    db.Remove(await db.ProtestoType.FindAsync(command.FromId));
                    break;
                default:
                    await MoveLinksAsync(db.ProtestoProtestoPlace, x => x.ProtestoPlaceId, x => x.ProtestoId,
                        command.FromId, command.IntoId, (x, id) => x.ProtestoPlaceId = id);
                    db.Remove(await db.ProtestoPlace.FindAsync(command.FromId));
                    break;
            }
            await db.SaveChangesAsync();
            return new MergeResult(MergeStatus.Ok, affected);
        }

        private async Task<bool> BothExistAsync(MergeDefinitionsCommand command)
        {
            int[] ids = [command.FromId, command.IntoId];
            var found = command.Kind switch
            {
                DefinitionKind.Companies => await db.Company.CountAsync(c => ids.Contains(c.Id) && !c.Deleted),
                DefinitionKind.Corporations => await db.Corporation.CountAsync(c => ids.Contains(c.Id)),
                DefinitionKind.Reasons => await db.ResistanceReason.CountAsync(c => ids.Contains(c.Id)),
                DefinitionKind.ProtestTypes => await db.ProtestoType.CountAsync(c => ids.Contains(c.Id)),
                _ => await db.ProtestoPlace.CountAsync(c => ids.Contains(c.Id)),
            };
            return found == 2;
        }

        // A link row of From moves to Into, unless its owner (case or protest) already has Into: then it goes.
        private static async Task MoveLinksAsync<T>(
            DbSet<T> links, Expression<Func<T, int>> valueOf, Expression<Func<T, int>> ownerOf, int fromId, int intoId, Action<T, int> setValue)
            where T : class
        {
            // Only the rows of From and Into, filtered in the database.
            var isFromOrInto = Expression.Lambda<Func<T, bool>>(
                Expression.OrElse(
                    Expression.Equal(valueOf.Body, Expression.Constant(fromId)),
                    Expression.Equal(valueOf.Body, Expression.Constant(intoId))),
                valueOf.Parameters);
            var rows = await links.Where(isFromOrInto).ToListAsync();
            var value = valueOf.Compile();
            var owner = ownerOf.Compile();

            var ownersWithInto = rows.Where(x => value(x) == intoId).Select(owner).ToHashSet();
            var seen = new HashSet<int>();
            foreach (var row in rows.Where(x => value(x) == fromId))
            {
                if (ownersWithInto.Contains(owner(row)) || !seen.Add(owner(row)))
                    links.Remove(row);
                else
                    setValue(row, intoId);
            }
        }

        private async Task MergeCompaniesAsync(int fromId, int intoId)
        {
            foreach (var r in await db.Resistance.Where(r => r.CompanyId == fromId || r.MainCompanyId == fromId).ToListAsync())
            {
                if (r.CompanyId == fromId) r.CompanyId = intoId;
                if (r.MainCompanyId == fromId) r.MainCompanyId = intoId;
            }

            // Subcontractor links: repoint, then drop self-links and duplicates.
            var links = await db.CompanyOutsourceCompany
                .Where(l => l.CompanyId == fromId || l.OutsourceCompanyId == fromId || l.CompanyId == intoId || l.OutsourceCompanyId == intoId)
                .ToListAsync();
            foreach (var link in links)
            {
                if (link.CompanyId == fromId) link.CompanyId = intoId;
                if (link.OutsourceCompanyId == fromId) link.OutsourceCompanyId = intoId;
            }
            var kept = new HashSet<(int, int)>();
            foreach (var link in links.OrderBy(l => l.Id))
            {
                if (link.CompanyId == link.OutsourceCompanyId || !kept.Add((link.CompanyId, link.OutsourceCompanyId)))
                    db.CompanyOutsourceCompany.Remove(link);
            }

            (await db.Company.FindAsync(fromId))!.Deleted = true;
        }
    }
}
