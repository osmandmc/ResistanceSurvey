using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using RS.COMMON.Entities;
using RS.EF;

namespace RS.Tests;

public class CollationTests
{
    // Collations live in the design-time model (what migrations are made from), not the runtime one.
    private static IModel DesignModel()
    {
        var db = new RSDBContext(new DbContextOptionsBuilder<RSDBContext>().UseSqlServer("Server=none;Database=none").Options);
        return db.GetService<IDesignTimeModel>().Model;
    }

    // The database default ignores case only for Latin letters; all text follows Turkish rules (F9).
    [Fact]
    public void EveryTextColumnUsesTheTurkishCollation()
    {
        var latin = DesignModel().GetEntityTypes()
            .SelectMany(e => e.GetProperties().Where(p => p.ClrType == typeof(string)).Select(p => (Entity: e.ClrType.Name, Property: p)))
            .Where(x => x.Property.GetCollation() != RSDBContext.TurkishCollation)
            .Select(x => $"{x.Entity}.{x.Property.Name}")
            .ToList();

        // Hashes and the security stamp are compared exactly: the article id, and an account's (F4a); the activity log's
        // codes and article id too (F4b); the R31 migration record's step code and id list.
        Assert.Equal(
            [
                $"{nameof(ActivityEntry)}.{nameof(ActivityEntry.Action)}", $"{nameof(ActivityEntry)}.{nameof(ActivityEntry.ArticleId)}",
                $"{nameof(ActivityEntry)}.{nameof(ActivityEntry.Subject)}",
                $"{nameof(AppUser)}.{nameof(AppUser.InviteTokenHash)}", $"{nameof(AppUser)}.{nameof(AppUser.PasswordHash)}",
                $"{nameof(AppUser)}.{nameof(AppUser.SecurityStamp)}",
                $"{nameof(CaseMigrationRecord)}.{nameof(CaseMigrationRecord.RemovedCorporationIds)}", $"{nameof(CaseMigrationRecord)}.{nameof(CaseMigrationRecord.Step)}",
                $"{nameof(News)}.{nameof(News.ArticleId)}",
            ],
            latin.Order());
    }
}
