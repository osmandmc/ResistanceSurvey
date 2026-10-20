using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.COMMON.Entities;
using RS.EF;
using RS.MVC.Features.Review;

namespace RS.MVC.Features.Definitions
{
    /// <summary>Values of one list whose names come down to the same key; the first item is the suggested one to keep.</summary>
    public record DuplicateGroup(string Key, IReadOnlyList<DefinitionItem> Items);

    /// <summary>
    /// The comparison key for likely duplicates (R12a-2): Turkish lower case, punctuation removed, Turkish letters
    /// folded ("Ücret" and "Ucret" match), and for companies the legal-form words dropped ("Örnek Tekstil A.Ş." and
    /// "ÖRNEK TEKSTİL SAN. TİC. LTD. ŞTİ." match).
    /// </summary>
    public static class DuplicateKey
    {
        public static string For(DefinitionKind kind, string name)
        {
            var words = kind == DefinitionKind.Companies
                ? CompanyNameMatcher.CoreWords(name)
                : CompanyNameMatcher.Normalize(name).Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            return Fold(string.Join(' ', words));
        }

        private static string Fold(string text)
        {
            var folded = new StringBuilder(text.Length);
            foreach (var c in text.Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    folded.Append(c == 'ı' ? 'i' : c);
            }
            return folded.ToString();
        }
    }

    /// <summary>
    /// Groups of likely duplicates in one list, biggest first. A group is left out once every pair in it was marked
    /// "aynı değil". The suggested value to keep comes first: approved, then most used, then the oldest.
    /// </summary>
    public class FindDuplicatesHandler(RSDBContext db)
    {
        public async Task<IReadOnlyList<DuplicateGroup>> Handle(DefinitionKind kind)
        {
            var values = await NamesAsync(kind);
            var notDuplicate = (await db.DefinitionNotDuplicate.Where(x => x.Kind == Slug(kind)).Select(x => new { x.FirstId, x.SecondId }).ToListAsync())
                .Select(x => (x.FirstId, x.SecondId)).ToHashSet();

            var groups = values
                .Where(v => !string.IsNullOrWhiteSpace(v.Name))
                .GroupBy(v => DuplicateKey.For(kind, v.Name))
                .Where(g => g.Key.Length > 0 && g.Count() > 1)
                .Where(g => Pairs(g.Select(v => v.Id)).Any(p => !notDuplicate.Contains(p)))
                .ToList();
            if (groups.Count == 0)
                return [];

            var usage = await DefinitionUsage.CountAsync(db, kind, groups.SelectMany(g => g.Select(v => v.Id)).ToList());
            return groups
                .Select(g => new DuplicateGroup(g.Key, g
                    .Select(v => new DefinitionItem(v.Id, v.Name, v.Approved, usage.GetValueOrDefault(v.Id)))
                    .OrderByDescending(i => i.Approved ?? true).ThenByDescending(i => i.Usage).ThenBy(i => i.Id)
                    .ToList()))
                .OrderByDescending(g => g.Items.Count).ThenBy(g => g.Key, StringComparer.Ordinal)
                .ToList();
        }

        private async Task<List<(int Id, string Name, bool? Approved)>> NamesAsync(DefinitionKind kind) => kind switch
        {
            DefinitionKind.Companies => (await db.Company.Where(c => !c.Deleted).Select(c => new { c.Id, c.Name }).ToListAsync())
                .Select(c => (c.Id, c.Name, (bool?)null)).ToList(),
            DefinitionKind.Corporations => (await db.Corporation.Select(c => new { c.Id, c.Name, c.Approved }).ToListAsync())
                .Select(c => (c.Id, c.Name, (bool?)c.Approved)).ToList(),
            DefinitionKind.Reasons => (await db.ResistanceReason.Select(c => new { c.Id, c.Name, c.Approved }).ToListAsync())
                .Select(c => (c.Id, c.Name, (bool?)c.Approved)).ToList(),
            DefinitionKind.ProtestTypes => (await db.ProtestoType.Select(c => new { c.Id, c.Name, c.Approved }).ToListAsync())
                .Select(c => (c.Id, c.Name, (bool?)c.Approved)).ToList(),
            _ => (await db.ProtestoPlace.Select(c => new { c.Id, c.Name, c.Approved }).ToListAsync())
                .Select(c => (c.Id, c.Name, (bool?)c.Approved)).ToList(),
        };

        /// <summary>Every pair of ids, smaller first, as stored in DefinitionNotDuplicate.</summary>
        public static IEnumerable<(int, int)> Pairs(IEnumerable<int> ids)
        {
            var list = ids.Distinct().Order().ToList();
            for (var i = 0; i < list.Count; i++)
                for (var j = i + 1; j < list.Count; j++)
                    yield return (list[i], list[j]);
        }

        public static string Slug(DefinitionKind kind) => kind switch
        {
            DefinitionKind.Companies => "companies",
            DefinitionKind.Corporations => "corporations",
            DefinitionKind.Reasons => "reasons",
            DefinitionKind.ProtestTypes => "protest-types",
            _ => "protest-places",
        };
    }

    /// <summary>"Aynı değil": these values look alike but differ; they are not suggested together again.</summary>
    public record DismissDuplicatesCommand(DefinitionKind Kind, IReadOnlyList<int> Ids);

    public class DismissDuplicatesHandler(RSDBContext db)
    {
        public async Task Handle(DismissDuplicatesCommand command)
        {
            var slug = FindDuplicatesHandler.Slug(command.Kind);
            var known = (await db.DefinitionNotDuplicate.Where(x => x.Kind == slug).Select(x => new { x.FirstId, x.SecondId }).ToListAsync())
                .Select(x => (x.FirstId, x.SecondId)).ToHashSet();
            foreach (var (first, second) in FindDuplicatesHandler.Pairs(command.Ids ?? []).Where(p => !known.Contains(p)))
                db.DefinitionNotDuplicate.Add(new DefinitionNotDuplicate { Kind = slug, FirstId = first, SecondId = second });
            await db.SaveChangesAsync();
        }
    }

    /// <summary>Merge several values into one (a duplicate group), all or nothing.</summary>
    public record MergeGroupCommand(DefinitionKind Kind, int IntoId, IReadOnlyList<int> FromIds);

    public class MergeGroupHandler(RSDBContext db, MergeDefinitionsHandler merge)
    {
        public async Task<MergeResult> Handle(MergeGroupCommand command)
        {
            var fromIds = (command.FromIds ?? []).Where(id => id != command.IntoId).Distinct().ToList();
            if (fromIds.Count == 0)
                return new MergeResult(MergeStatus.SameValue);

            // One transaction around the merges (each saves on its own); the in-memory test database has none.
            await using var transaction = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync() : null;
            var affected = 0;
            foreach (var fromId in fromIds)
            {
                var result = await merge.Handle(new MergeDefinitionsCommand(command.Kind, fromId, command.IntoId));
                if (result.Status != MergeStatus.Ok)
                    return result; // disposing the transaction rolls the earlier merges back
                affected += result.Affected;
            }
            if (transaction != null)
                await transaction.CommitAsync();
            return new MergeResult(MergeStatus.Ok, affected);
        }
    }
}
