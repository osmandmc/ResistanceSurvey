using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using RS.COMMON.Entities;
using RS.EF;

namespace RS.Tests;

/// <summary>The model and the migrations stay in step (F12): a model change without its migration fails here.</summary>
public class SchemaTests
{
    private static RSDBContext SqlServer() =>
        new(new DbContextOptionsBuilder<RSDBContext>().UseSqlServer("Server=none;Database=none").Options);

    [Fact]
    public void EveryModelChangeHasItsMigration()
    {
        using var db = SqlServer();

        Assert.False(db.Database.HasPendingModelChanges(), "The model differs from the last migration: run 'dotnet ef migrations add …'.");
    }

    [Fact]
    public void OnlyAnOwnerTakesItsPartsAlongOnDelete()
    {
        using var db = SqlServer();
        var model = db.GetService<IDesignTimeModel>().Model;

        var cascades = model.GetEntityTypes().SelectMany(e => e.GetForeignKeys())
            .Where(fk => fk.DeleteBehavior == DeleteBehavior.Cascade)
            .Select(fk => fk.PrincipalEntityType.ClrType.Name)
            .Distinct()
            .Order();

        // A case its protests and links, a protest its links and locations, a news item its case links. Never a list value.
        Assert.Equal([nameof(News), nameof(Protesto), nameof(Resistance)], cascades);
    }
}
