using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using RS.COMMON.Entities;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;

namespace RS.Tests;

public class CollationTests
{
    // The database default ignores case only for Latin letters; searched text must follow Turkish rules (F9).
    [Theory]
    [InlineData(typeof(Resistance), nameof(Resistance.Name))]
    [InlineData(typeof(Resistance), nameof(Resistance.Description))]
    [InlineData(typeof(Company), nameof(Company.Name))]
    [InlineData(typeof(Corporation), nameof(Corporation.Name))]
    [InlineData(typeof(News), nameof(News.Header))]
    public void SearchedTextUsesTheTurkishCollation(Type entity, string property)
    {
        using var db = new RSDBContext(new DbContextOptionsBuilder<RSDBContext>().UseSqlServer("Server=none;Database=none").Options);

        // Collations live in the design-time model (what migrations are made from), not the runtime one.
        var model = db.GetService<IDesignTimeModel>().Model;
        Assert.Equal(RSDBContext.TurkishCollation, model.FindEntityType(entity)!.FindProperty(property)!.GetCollation());
    }
}
