using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RS.EF;
using RS.MVC.Applications;
using RS.MVC.Features.Cases;
using RS.MVC.Features.Review;
using RS.MVC.Models;

namespace RS.Tests;

public class CreateCaseTests
{
    private readonly RSDBContext _db = new(new DbContextOptionsBuilder<RSDBContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
        .Options);

    private CreateCaseHandler Handler() => new(_db, new CaseWriter(_db), new ListValueResolver(_db));

    [Fact]
    public async Task ACaseWithoutAnArticleGetsItsProtestAndNoArticle()
    {
        var input = ReviewValidationTests.CompleteNewCase();
        input.Case!.NewReasonNames = ["Mobbing"];

        var result = await Handler().Handle(new CreateCaseCommand(input.Case, input.Protest!, "ayse"));

        Assert.Equal(CaseCommandStatus.Ok, result.Status);
        var r = _db.Resistance.Include(x => x.Protestos).Include(x => x.ResistanceNews).Include(x => x.ResistanceResistanceReasons).Single();
        Assert.Equal((result.Id, "ayse"), ((int?)r.Id, r.Creator));
        Assert.Single(r.Protestos);
        Assert.Empty(r.ResistanceNews);
        Assert.Equal(2, r.ResistanceResistanceReasons.Count); // the picked one and the typed one
        Assert.Empty(_db.News);
    }

    [Fact]
    public async Task MissingFieldsCreateNothing()
    {
        var result = await Handler().Handle(new CreateCaseCommand(new CaseInput(), new ProtestInput(), "ayse"));

        Assert.Equal(CaseCommandStatus.Invalid, result.Status);
        Assert.Contains("protest.startDate", result.Errors!.Keys);
        Assert.Empty(_db.Resistance);
    }
}
