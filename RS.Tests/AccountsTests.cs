using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RS.COMMON.Constants;
using RS.COMMON.Entities;
using RS.EF;
using RS.MVC.Features.Accounts;
using RS.MVC.Models;

namespace RS.Tests;

public class AccountsTests
{
    private readonly RSDBContext _db = new(new DbContextOptionsBuilder<RSDBContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private readonly Clock _clock = new();

    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private Task<SignInResult> SignIn(string username, string password, string adminPass = "") =>
        new SignInHandler(_db, _clock, Options.Create(new AppSettings { AdminPass = adminPass })).Handle(username, password);

    private Task<LinkResult> Create(string username, string role = Role.User) =>
        new CreateUserHandler(_db, _clock).Handle(new CreateUserCommand(username, role, "admin"));

    private Task<AcceptResult> Accept(string token, string password) => new AcceptInvitationHandler(_db, _clock).Handle(token, password);

    private Task<LinkResult> Change(int id, string? role = null, bool? disabled = null, int acting = 999) =>
        new ChangeUserHandler(_db, _clock).Handle(new ChangeUserCommand(id, role!, disabled, acting));

    private async Task<AppUser> Invited(string username, string password = "doğru-şifre-123")
    {
        var created = await Create(username);
        await Accept(created.Token, password);
        return await _db.AppUser.SingleAsync(u => u.Username == username);
    }

    [Fact]
    public async Task TheFirstSignInWithTheConfiguredPasswordCreatesTheAdmin()
    {
        var first = await SignIn("admin", "ilk-şifre", adminPass: "ilk-şifre");

        Assert.Null(first.Error);
        var admin = await _db.AppUser.SingleAsync();
        Assert.Equal((SignInHandler.FirstAdmin, Role.Admin), (admin.Username, admin.Role));
        Assert.NotEqual("ilk-şifre", admin.PasswordHash);

        // From now on the account's own password counts; the setting would not create a second one.
        Assert.Null((await SignIn("admin", "ilk-şifre", adminPass: "başka")).Error);
        _db.AppUser.RemoveRange(_db.AppUser);
        await _db.SaveChangesAsync();
        Assert.NotNull((await SignIn("admin", "yanlış", adminPass: "ilk-şifre")).Error);
        Assert.Empty(_db.AppUser);
    }

    [Fact]
    public async Task WithoutTheSettingThereIsNoFirstAdmin()
    {
        Assert.Equal(Accounts.WrongPassword, (await SignIn("admin", "", adminPass: "")).Error);
        Assert.Empty(_db.AppUser);
    }

    [Fact]
    public async Task AnInvitedPersonSetsTheirPasswordOnceThroughTheLink()
    {
        var created = await Create("ayse");
        Assert.Equal("invited", created.User.State);
        Assert.Equal(43, created.Token.Length); // 32 random bytes, Base64Url
        var stored = await _db.AppUser.SingleAsync();
        Assert.NotEqual(created.Token, stored.InviteTokenHash);

        var invitation = await new GetInvitationHandler(_db, _clock).Handle(created.Token);
        Assert.Equal(new Invitation("ayse", false), invitation);
        Assert.Equal($"Şifre en az {Accounts.MinPasswordLength} karakter olmalı.", (await Accept(created.Token, "kısa")).Error);

        var accepted = await Accept(created.Token, "uzun-bir-şifre");

        Assert.Null(accepted.Error);
        Assert.Equal(Accounts.LinkNotValid, (await Accept(created.Token, "başka-bir-şifre")).Error);
        Assert.Null(await new GetInvitationHandler(_db, _clock).Handle(created.Token));
        Assert.Null((await SignIn("ayse", "uzun-bir-şifre")).Error);
        Assert.Equal(Accounts.WrongPassword, (await SignIn("ayse", "başka-bir-şifre")).Error);
    }

    [Fact]
    public async Task ALinkExpiresAfterSevenDays()
    {
        var created = await Create("ayse");

        _clock.Now += Accounts.LinkValidFor + TimeSpan.FromMinutes(1);

        Assert.Equal(Accounts.LinkNotValid, (await Accept(created.Token, "uzun-bir-şifre")).Error);
        Assert.Equal("linkExpired", (await new ListUsersHandler(_db, _clock).Handle()).Single().State);
    }

    [Fact]
    public async Task ANewLinkReplacesTheOldOneAndTheOldPasswordWorksUntilItIsUsed()
    {
        var user = await Invited("ayse", "eski-şifre-123");
        var first = await new NewLinkHandler(_db, _clock).Handle(user.Id);
        var second = await new NewLinkHandler(_db, _clock).Handle(user.Id);

        Assert.NotEqual(first.Token, second.Token);
        Assert.Equal(Accounts.LinkNotValid, (await Accept(first.Token, "yeni-şifre-123")).Error);
        Assert.Null((await SignIn("ayse", "eski-şifre-123")).Error);

        Assert.Null((await Accept(second.Token, "yeni-şifre-123")).Error);
        Assert.NotNull((await SignIn("ayse", "eski-şifre-123")).Error);
        Assert.Null((await SignIn("ayse", "yeni-şifre-123")).Error);
    }

    [Fact]
    public async Task FiveWrongPasswordsLockTheAccountForFifteenMinutes()
    {
        await Invited("ayse");
        for (var i = 1; i < Accounts.MaxFailedSignIns; i++)
            Assert.Equal(Accounts.WrongPassword, (await SignIn("ayse", "yanlış")).Error);

        Assert.StartsWith("Çok fazla hatalı deneme", (await SignIn("ayse", "yanlış")).Error);
        Assert.StartsWith("Çok fazla hatalı deneme", (await SignIn("ayse", "doğru-şifre-123")).Error);

        _clock.Now += Accounts.LockFor;
        Assert.Null((await SignIn("ayse", "doğru-şifre-123")).Error);
    }

    [Fact]
    public async Task UsernamesAreCheckedAndUnique()
    {
        Assert.NotNull((await Create("a b")).Error);
        Assert.NotNull((await Create("ab")).Error);
        Assert.NotNull((await Create("ayse", role: "Root")).Error);
        Assert.Null((await Create("ayşe.k")).Error);
        Assert.Equal("“ayşe.k” kullanıcı adı zaten var.", (await Create(" ayşe.k ")).Error);
    }

    [Fact]
    public async Task DisablingEndsSessionsAndClosesTheLink()
    {
        var user = await Invited("ayse");
        var session = Accounts.Principal(user);
        var link = await new NewLinkHandler(_db, _clock).Handle(user.Id);
        var validate = new ValidateSessionHandler(_db);
        Assert.Same(session, await validate.Handle(session));

        await Change(user.Id, disabled: true);

        Assert.Null(await validate.Handle(session));
        Assert.Equal(Accounts.LinkNotValid, (await Accept(link.Token, "yeni-şifre-123")).Error);
        Assert.Equal(Accounts.WrongPassword, (await SignIn("ayse", "doğru-şifre-123")).Error);
        Assert.NotNull((await new NewLinkHandler(_db, _clock).Handle(user.Id)).Error);

        await Change(user.Id, disabled: false);
        Assert.Null((await SignIn("ayse", "doğru-şifre-123")).Error);
        Assert.Null(await validate.Handle(session)); // the old session stays ended
    }

    [Fact]
    public async Task ANewPasswordEndsOlderSessions()
    {
        var user = await Invited("ayse");
        var session = Accounts.Principal(user);
        var link = await new NewLinkHandler(_db, _clock).Handle(user.Id);

        await Accept(link.Token, "yeni-şifre-123");

        Assert.Null(await new ValidateSessionHandler(_db).Handle(session));
    }

    [Fact]
    public async Task ARoleChangeAppliesToTheOpenSession()
    {
        var user = await Invited("ayse");
        var session = Accounts.Principal(user);

        await Change(user.Id, role: Role.Admin);

        var renewed = await new ValidateSessionHandler(_db).Handle(session);
        Assert.Equal(Role.Admin, renewed!.FindFirstValue(ClaimTypes.Role));
    }

    [Fact]
    public async Task AYoneticiCannotChangeTheirOwnAccount()
    {
        var admin = await Invited("yonetici");

        var result = await Change(admin.Id, disabled: true, acting: admin.Id);

        Assert.NotNull(result.Error);
        Assert.False((await _db.AppUser.SingleAsync()).Disabled);
    }
}
