using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.COMMON.Entities;
using RS.EF;
using RS.MVC.Features.Activity;

namespace RS.MVC.Features.Accounts
{
    /// <summary>Whose link it is, shown on the page where they set their password.</summary>
    public record Invitation(string Username, bool HasPassword);

    /// <summary>The account whose password was set (now signed in), or why not.</summary>
    public record AcceptResult(AppUser User, string Error);

    internal static class OpenLinks
    {
        /// <summary>The enabled account whose unexpired link has this token, or null.</summary>
        public static Task<AppUser> FindAsync(RSDBContext db, string token, DateTime now)
        {
            if (string.IsNullOrEmpty(token))
                return Task.FromResult<AppUser>(null);
            var hash = Accounts.HashToken(token);
            return db.AppUser.FirstOrDefaultAsync(u => u.InviteTokenHash == hash && !u.Disabled && u.InviteExpiresAt > now);
        }
    }

    /// <summary>GET: the link's account, or null when the link is used, expired, replaced or unknown (one answer for all).</summary>
    public class GetInvitationHandler(RSDBContext db, TimeProvider clock)
    {
        public async Task<Invitation> Handle(string token)
        {
            var user = await OpenLinks.FindAsync(db, token, clock.GetUtcNow().UtcDateTime);
            return user == null ? null : new Invitation(user.Username, user.PasswordHash != null);
        }
    }

    /// <summary>The person sets their password through the link; the link is then closed and older sessions end.</summary>
    public class AcceptInvitationHandler(RSDBContext db, TimeProvider clock)
    {
        public async Task<AcceptResult> Handle(string token, string password)
        {
            var now = clock.GetUtcNow().UtcDateTime;
            var user = await OpenLinks.FindAsync(db, token, now);
            if (user == null)
                return new AcceptResult(null, Accounts.LinkNotValid);
            var problem = Accounts.PasswordProblem(password);
            if (problem != null)
                return new AcceptResult(null, problem);

            Accounts.SetPassword(user, password);
            Accounts.CloseLink(user);
            user.SecurityStamp = Accounts.NewStamp();
            user.FailedSignIns = 0;
            user.LockedUntil = null;
            user.LastSignInAt = now;
            db.LogActivity(Actions.UserPassword, ActivityLog.Quoted(user.Username), subject: "user", subjectId: user.Id, username: user.Username);
            await db.SaveChangesAsync();
            return new AcceptResult(user, null);
        }
    }
}
