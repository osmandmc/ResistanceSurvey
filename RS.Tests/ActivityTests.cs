using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RS.COMMON.Constants;
using RS.COMMON.Entities;
using RS.COMMON.Entities.LookupEntity;
using RS.EF;
using RS.MVC.Features.Accounts;
using RS.MVC.Features.Activity;
using RS.MVC.Features.Definitions;

namespace RS.Tests;

/// <summary>F4b: the activity log. Inbox decisions and case edits are checked in ReviewTests and CaseDetailTests.</summary>
public class ActivityTests
{
    private readonly TestUser _user = new() { Name = "ayse" };
    private readonly RSDBContext _db;

    public ActivityTests() =>
        _db = new(_user.Track(new DbContextOptionsBuilder<RSDBContext>().UseInMemoryDatabase(Guid.NewGuid().ToString())).Options);

    private ActivityEntry Last() => _db.ActivityEntry.OrderBy(e => e.Id).Last();

    [Fact]
    public async Task AMergeNamesBothValuesAndHowManyCasesItMoved()
    {
        var typo = new Corporation { Name = "Disk Genel İş", Approved = false };
        var kept = new Corporation { Name = "DİSK Genel-İş", Approved = true };
        _db.AddRange(typo, kept);
        _db.Resistance.Add(new Resistance { Name = "Vaka", ResistanceCorporations = [new ResistanceCorporation { Corporation = typo }] });
        await _db.SaveChangesAsync();

        await new MergeDefinitionsHandler(_db).Handle(new MergeDefinitionsCommand(DefinitionKind.Corporations, typo.Id, kept.Id));

        var entry = Last();
        Assert.Equal(("ayse", Actions.DefinitionMerge, "corporations", kept.Id), (entry.Username, entry.Action, entry.Subject, entry.SubjectId));
        Assert.Equal("Kurum “Disk Genel İş” → “DİSK Genel-İş” (1 vaka)", entry.Summary);
    }

    [Fact]
    public async Task RenamesApprovalsAndDeletionsKeepTheName()
    {
        var reason = new ResistanceReason { Name = "Ucret", Approved = false };
        _db.Add(reason);
        await _db.SaveChangesAsync();

        await new UpdateDefinitionHandler(_db).Handle(new UpdateDefinitionCommand(DefinitionKind.Reasons, reason.Id, "Ücret"));
        Assert.Equal("Vaka nedeni “Ucret” → “Ücret”", Last().Summary);
        await new ApproveDefinitionHandler(_db).Handle(new ApproveDefinitionCommand(DefinitionKind.Reasons, reason.Id));
        Assert.Equal((Actions.DefinitionApprove, "Vaka nedeni “Ücret”"), (Last().Action, Last().Summary));
        _user.Name = "mehmet";
        await new DeleteDefinitionHandler(_db).Handle(new DeleteDefinitionCommand(DefinitionKind.Reasons, reason.Id));
        Assert.Equal(("mehmet", Actions.DefinitionDelete, "Vaka nedeni “Ücret”"), (Last().Username, Last().Action, Last().Summary));
    }

    [Fact]
    public async Task AFailedChangeLeavesNoLine()
    {
        var reason = new ResistanceReason { Name = "Ücret", Approved = true };
        _db.Add(reason);
        _db.Resistance.Add(new Resistance { Name = "Vaka", ResistanceResistanceReasons = [new ResistanceResistanceReason { ResistanceReason = reason }] });
        await _db.SaveChangesAsync();

        var result = await new DeleteDefinitionHandler(_db).Handle(new DeleteDefinitionCommand(DefinitionKind.Reasons, reason.Id));

        Assert.Equal(ChangeStatus.InUse, result.Status);
        Assert.Empty(_db.ActivityEntry);
    }

    [Fact]
    public async Task AccountChangesAreLoggedOnlyWhenSomethingChanged()
    {
        _user.Name = "admin";
        var clock = TimeProvider.System;
        var created = await new CreateUserHandler(_db, clock).Handle(new CreateUserCommand("ayse", Role.User, "admin"));
        var id = created.User.Id;
        var change = new ChangeUserHandler(_db, clock);

        await change.Handle(new ChangeUserCommand(id, Role.Admin, null, 999));
        await change.Handle(new ChangeUserCommand(id, Role.Admin, false, 999)); // nothing new
        await change.Handle(new ChangeUserCommand(id, null, true, 999));

        Assert.Equal(
            [
                ("admin", Actions.UserCreate, "“ayse” (araştırmacı)"),
                ("admin", Actions.UserRole, "“ayse”: araştırmacı → yönetici"),
                ("admin", Actions.UserDisable, "“ayse”"),
            ],
            _db.ActivityEntry.OrderBy(e => e.Id).AsEnumerable().Select(e => (e.Username, e.Action, e.Summary)));
    }

    [Fact]
    public async Task APersonSettingTheirPasswordIsNamedByTheAccount()
    {
        var created = await new CreateUserHandler(_db, TimeProvider.System).Handle(new CreateUserCommand("ayse", Role.User, "admin"));
        _user.Name = null; // the welcome page: nobody is signed in yet

        await new AcceptInvitationHandler(_db, TimeProvider.System).Handle(created.Token, "uzun-bir-şifre");

        Assert.Equal(("ayse", Actions.UserPassword), (Last().Username, Last().Action));
    }

    [Fact]
    public async Task KayitlarFiltersByPersonAndArea()
    {
        _db.LogActivity(Actions.ArticleDismiss, "Haber 1");
        _db.LogActivity(Actions.ProtestAdd, "01.10.2026 eylemi", caseId: 7);
        await _db.SaveChangesAsync();
        _user.Name = "mehmet";
        _db.LogActivity(Actions.CaseUpdate, "Vaka", caseId: 7);
        _db.LogActivity(Actions.DefinitionMerge, "Kurum");
        await _db.SaveChangesAsync();
        var list = new ListActivityHandler(_db);

        Assert.Equal(["Vaka", "01.10.2026 eylemi"], (await list.Handle(new ListActivityQuery(Area: "cases"))).Items.Select(i => i.Summary));
        Assert.Equal(["Kurum", "Vaka"], (await list.Handle(new ListActivityQuery("mehmet"))).Items.Select(i => i.Summary));
        var all = await list.Handle(new ListActivityQuery());
        Assert.Equal(4, all.Total);
        Assert.Equal(["ayse", "mehmet"], all.Usernames);
        Assert.False(ListActivityHandler.IsArea("everything"));
    }

    // The guard: a request that changed data without a log line is reported.

    private sealed class Warnings : ILogger<ActivityGuard>
    {
        public List<string> Lines { get; } = [];
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            Lines.Add(formatter(state, exception));
    }

    /// <summary>Runs <paramref name="action"/> as an endpoint behind the guard; returns the warnings.</summary>
    private async Task<List<string>> Guarded(Func<Task> action, params object[] endpointMetadata)
    {
        var warnings = new Warnings();
        _user.Http.HttpContext!.Items.Clear(); // a new request
        _user.Http.HttpContext!.Request.Method = "POST";
        _user.Http.HttpContext.Request.Path = "/api/test";
        var descriptor = new ActionDescriptor { EndpointMetadata = endpointMetadata.ToList() };
        var context = new ActionExecutingContext(
            new ActionContext(_user.Http.HttpContext, new RouteData(), descriptor), [], new Dictionary<string, object?>(), new object());
        await new ActivityGuard(warnings).OnActionExecutionAsync(context, async () =>
        {
            await action();
            return new ActionExecutedContext(context, [], new object());
        });
        return warnings.Lines;
    }

    [Fact]
    public async Task AChangeWithoutALineIsReported()
    {
        var warnings = await Guarded(async () =>
        {
            _db.Add(new ResistanceReason { Name = "Ücret" });
            await _db.SaveChangesAsync();
        });

        Assert.Equal("POST /api/test changed data but wrote no activity line (F4b): add one, or mark the endpoint [NotLogged]", Assert.Single(warnings));
    }

    [Fact]
    public async Task AChangeWithItsLineAMarkedEndpointOrNoChangeIsQuiet()
    {
        Assert.Empty(await Guarded(async () =>
        {
            _db.Add(new ResistanceReason { Name = "Ücret" });
            _db.LogActivity(Actions.DefinitionUpdate, "Vaka nedeni “Ücret”");
            await _db.SaveChangesAsync();
        }));
        Assert.Empty(await Guarded(async () =>
        {
            _db.Add(new ResistanceReason { Name = "Grev" });
            await _db.SaveChangesAsync();
        }, new NotLoggedAttribute("test")));
        Assert.Empty(await Guarded(async () => await _db.ResistanceReason.CountAsync()));
    }
}
