using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RS.COMMON.Constants;
using RS.COMMON.Entities;
using RS.EF;
using RS.MVC.Models;

namespace RS.MVC.Features.Accounts
{
    /// <summary>The account that signed in, or why not (shown as is).</summary>
    public record SignInResult(AppUser User, string Error);

    /// <summary>
    /// Username and password. Five wrong passwords in a row lock the account for 15 minutes. The very first sign-in,
    /// while no account exists, is "admin" with AppSettings:AdminPass: it creates that account as yönetici; after
    /// that the setting is ignored.
    /// </summary>
    public class SignInHandler(RSDBContext db, TimeProvider clock, IOptions<AppSettings> settings)
    {
        public const string FirstAdmin = "admin";

        public async Task<SignInResult> Handle(string username, string password)
        {
            var now = clock.GetUtcNow().UtcDateTime;
            username = username?.Trim();

            if (await IsFirstAdminAsync(username, password))
            {
                var admin = new AppUser { Username = FirstAdmin, Role = Role.Admin, SecurityStamp = Accounts.NewStamp(), CreatedAt = now, LastSignInAt = now };
                Accounts.SetPassword(admin, password);
                db.AppUser.Add(admin);
                await db.SaveChangesAsync();
                return new SignInResult(admin, null);
            }

            var user = string.IsNullOrEmpty(username) ? null : await db.AppUser.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null || user.Disabled)
            {
                Accounts.CheckPassword(null, password);
                return new SignInResult(null, Accounts.WrongPassword);
            }
            if (user.LockedUntil > now)
                return new SignInResult(null, Locked);

            if (!Accounts.CheckPassword(user, password))
            {
                user.FailedSignIns++;
                var locked = user.FailedSignIns >= Accounts.MaxFailedSignIns;
                if (locked)
                {
                    user.FailedSignIns = 0;
                    user.LockedUntil = now + Accounts.LockFor;
                }
                await db.SaveChangesAsync();
                return new SignInResult(null, locked ? Locked : Accounts.WrongPassword);
            }

            user.FailedSignIns = 0;
            user.LockedUntil = null;
            user.LastSignInAt = now;
            await db.SaveChangesAsync();
            return new SignInResult(user, null);
        }

        private static string Locked => $"Çok fazla hatalı deneme. {Accounts.LockFor.TotalMinutes:0} dakika sonra tekrar deneyin.";

        private async Task<bool> IsFirstAdminAsync(string username, string password)
        {
            var configured = settings.Value.AdminPass;
            if (username != FirstAdmin || string.IsNullOrEmpty(configured) || string.IsNullOrEmpty(password))
                return false;
            var same = CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(configured));
            return same && !await db.AppUser.AnyAsync();
        }
    }

    /// <summary>
    /// Run for every request with a sign-in cookie: the account must still exist, be enabled and have the same
    /// security stamp (a new password or disabling ends older sessions). A changed role takes effect at once.
    /// </summary>
    public class ValidateSessionHandler(RSDBContext db)
    {
        /// <summary>The principal to use: the same, a fresh one (role changed), or null (sign out).</summary>
        public async Task<ClaimsPrincipal> Handle(ClaimsPrincipal principal)
        {
            if (!int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
                return null;
            var user = await db.AppUser.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if (user == null || user.Disabled || user.SecurityStamp != principal.FindFirstValue(Accounts.StampClaim))
                return null;
            return user.Role == principal.FindFirstValue(ClaimTypes.Role) && user.Username == principal.Identity?.Name
                ? principal
                : Accounts.Principal(user);
        }
    }
}
