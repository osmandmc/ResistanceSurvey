using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.COMMON.Constants;
using RS.COMMON.Entities;
using RS.EF;
using RS.MVC.Features.Activity;

namespace RS.MVC.Features.Accounts
{
    /// <summary>Where an account stands: "active", "invited" (link open), "linkExpired" (never set a password) or "disabled".</summary>
    public record UserRow(int Id, string Username, string Role, string State, DateTime? LastSignInAt, DateTime? LinkExpiresAt, DateTime CreatedAt, string CreatedBy)
    {
        public static UserRow Of(AppUser u, DateTime now) => new(
            u.Id, u.Username, u.Role,
            u.Disabled ? "disabled"
            : u.InviteTokenHash != null && u.InviteExpiresAt > now ? "invited"
            : u.PasswordHash == null ? "linkExpired"
            : "active",
            Utc(u.LastSignInAt),
            u.InviteTokenHash != null && u.InviteExpiresAt > now ? Utc(u.InviteExpiresAt) : null,
            Utc(u.CreatedAt).Value, u.CreatedBy);

        // Stored as UTC; the database gives them back without a kind, and the browser would read them as local time.
        private static DateTime? Utc(DateTime? value) => value is { } v ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : null;
    }

    /// <summary>The account and its new link's token (shown once; only its hash is kept), or why not.</summary>
    public record LinkResult(UserRow User, string Token, string Error);

    public record CreateUserCommand(string Username, string Role, string CreatedBy);

    /// <summary>Role and/or enabled state; null leaves it as is. ActingUserId: the yönetici making the change.</summary>
    public record ChangeUserCommand(int Id, string Role, bool? Disabled, int ActingUserId);

    /// <summary>Kullanıcılar (yönetici only): every account, newest first.</summary>
    public class ListUsersHandler(RSDBContext db, TimeProvider clock)
    {
        public async Task<IReadOnlyList<UserRow>> Handle()
        {
            var now = clock.GetUtcNow().UtcDateTime;
            var users = await db.AppUser.AsNoTracking().OrderBy(u => u.Disabled).ThenByDescending(u => u.CreatedAt).ToListAsync();
            return users.Select(u => UserRow.Of(u, now)).ToList();
        }
    }

    /// <summary>A new account with a link to set its password.</summary>
    public class CreateUserHandler(RSDBContext db, TimeProvider clock)
    {
        public async Task<LinkResult> Handle(CreateUserCommand command)
        {
            var username = command.Username?.Trim();
            var problem = Accounts.UsernameProblem(username) ?? RoleProblem(command.Role);
            if (problem != null)
                return new LinkResult(null, null, problem);
            if (await db.AppUser.AnyAsync(u => u.Username == username))
                return new LinkResult(null, null, $"“{username}” kullanıcı adı zaten var.");

            var now = clock.GetUtcNow().UtcDateTime;
            var user = new AppUser { Username = username, Role = command.Role, SecurityStamp = Accounts.NewStamp(), CreatedAt = now, CreatedBy = command.CreatedBy };
            var token = Accounts.OpenLink(user, now);
            db.AppUser.Add(user);
            await db.SaveChangesAsync();
            db.LogActivity(Actions.UserCreate, $"{ActivityLog.Quoted(username)} ({RoleLabel(command.Role)})", subject: "user", subjectId: user.Id);
            await db.SaveChangesAsync();
            return new LinkResult(UserRow.Of(user, now), token, null);
        }

        internal static string RoleProblem(string role) =>
            role is Role.Admin or Role.User ? null : "Rol yönetici ya da araştırmacı olmalı.";

        internal static string RoleLabel(string role) => role == Role.Admin ? "yönetici" : "araştırmacı";
    }

    /// <summary>
    /// "Yeni bağlantı": a fresh link for an invitation that expired or a forgotten password. The earlier link stops
    /// working; the current password keeps working until the person sets a new one.
    /// </summary>
    public class NewLinkHandler(RSDBContext db, TimeProvider clock)
    {
        public async Task<LinkResult> Handle(int id)
        {
            var user = await db.AppUser.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return null;
            if (user.Disabled)
                return new LinkResult(null, null, "Devre dışı bir hesaba bağlantı verilemez; önce etkinleştirin.");
            var now = clock.GetUtcNow().UtcDateTime;
            var token = Accounts.OpenLink(user, now);
            db.LogActivity(Actions.UserLink, ActivityLog.Quoted(user.Username), subject: "user", subjectId: user.Id);
            await db.SaveChangesAsync();
            return new LinkResult(UserRow.Of(user, now), token, null);
        }
    }

    /// <summary>
    /// Change a role, disable or enable. A yönetici cannot change their own account here, so there is always at least
    /// one enabled yönetici. Disabling closes the link and ends the account's sessions.
    /// </summary>
    public class ChangeUserHandler(RSDBContext db, TimeProvider clock)
    {
        public async Task<LinkResult> Handle(ChangeUserCommand command)
        {
            var user = await db.AppUser.FirstOrDefaultAsync(u => u.Id == command.Id);
            if (user == null)
                return null;
            if (user.Id == command.ActingUserId)
                return new LinkResult(null, null, "Kendi hesabınızın rolünü ya da durumunu değiştiremezsiniz.");
            var who = ActivityLog.Quoted(user.Username);
            if (command.Role != null && command.Role != user.Role)
            {
                var problem = CreateUserHandler.RoleProblem(command.Role);
                if (problem != null)
                    return new LinkResult(null, null, problem);
                db.LogActivity(Actions.UserRole,
                    $"{who}: {CreateUserHandler.RoleLabel(user.Role)} → {CreateUserHandler.RoleLabel(command.Role)}", subject: "user", subjectId: user.Id);
                user.Role = command.Role;
            }
            if (command.Disabled == true && !user.Disabled)
            {
                user.Disabled = true;
                user.SecurityStamp = Accounts.NewStamp();
                Accounts.CloseLink(user);
                db.LogActivity(Actions.UserDisable, who, subject: "user", subjectId: user.Id);
            }
            else if (command.Disabled == false && user.Disabled)
            {
                user.Disabled = false;
                db.LogActivity(Actions.UserEnable, who, subject: "user", subjectId: user.Id);
            }
            await db.SaveChangesAsync();
            return new LinkResult(UserRow.Of(user, clock.GetUtcNow().UtcDateTime), null, null);
        }
    }
}
