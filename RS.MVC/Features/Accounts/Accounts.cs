using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using RS.COMMON.Entities;

namespace RS.MVC.Features.Accounts
{
    /// <summary>
    /// Per-person accounts (F4a): what sign-in, the invitation links and the Kullanıcılar page share. Passwords are
    /// hashed with ASP.NET Core's PasswordHasher (PBKDF2); a link's token is 32 random bytes, stored only as SHA-256.
    /// </summary>
    public static class Accounts
    {
        public const int MinPasswordLength = 10;
        public const int MaxPasswordLength = 200;
        public const int MaxFailedSignIns = 5;
        public static readonly TimeSpan LockFor = TimeSpan.FromMinutes(15);
        public static readonly TimeSpan LinkValidFor = TimeSpan.FromDays(7);

        /// <summary>The cookie's copy of <see cref="AppUser.SecurityStamp"/>, checked on every request.</summary>
        public const string StampClaim = "rs:stamp";

        public const string WrongPassword = "Kullanıcı adı veya şifre hatalı.";
        public const string LinkNotValid = "Bu bağlantı artık geçerli değil; yöneticiden yeni bir bağlantı isteyin.";

        private static readonly PasswordHasher<AppUser> Hasher = new();

        // Verified when the username is unknown, so a wrong username takes as long as a wrong password.
        private static readonly string DummyHash = Hasher.HashPassword(new AppUser(), "not-a-password");

        private static readonly Regex UsernamePattern = new(@"^[\p{L}\p{N}._-]{3,50}$");

        public static void SetPassword(AppUser user, string password) => user.PasswordHash = Hasher.HashPassword(user, password);

        /// <summary>True when it matches; an old-format hash is replaced on the way.</summary>
        public static bool CheckPassword(AppUser user, string password)
        {
            if (user?.PasswordHash == null)
            {
                Hasher.VerifyHashedPassword(new AppUser(), DummyHash, password ?? "");
                return false;
            }
            var result = Hasher.VerifyHashedPassword(user, user.PasswordHash, password ?? "");
            if (result == PasswordVerificationResult.SuccessRehashNeeded)
                SetPassword(user, password);
            return result != PasswordVerificationResult.Failed;
        }

        /// <summary>Why the password cannot be used, or null.</summary>
        public static string PasswordProblem(string password) =>
            password == null || password.Length < MinPasswordLength ? $"Şifre en az {MinPasswordLength} karakter olmalı."
            : password.Length > MaxPasswordLength ? $"Şifre en fazla {MaxPasswordLength} karakter olabilir."
            : null;

        /// <summary>Why the username cannot be used, or null.</summary>
        public static string UsernameProblem(string username) =>
            username != null && UsernamePattern.IsMatch(username) ? null
            : "Kullanıcı adı 3–50 karakter olmalı: harf, rakam, nokta, alt çizgi ya da tire.";

        public static string NewStamp() => Guid.NewGuid().ToString("N");

        public static string HashToken(string token) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token ?? ""))).ToLowerInvariant();

        /// <summary>A new one-time link for the account; any earlier link stops working. Returns the token (shown once).</summary>
        public static string OpenLink(AppUser user, DateTime now)
        {
            var token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
            user.InviteTokenHash = HashToken(token);
            user.InviteExpiresAt = now + LinkValidFor;
            return token;
        }

        public static void CloseLink(AppUser user)
        {
            user.InviteTokenHash = null;
            user.InviteExpiresAt = null;
        }

        public static ClaimsPrincipal Principal(AppUser user) =>
            new(new ClaimsIdentity(
                new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new(ClaimTypes.Name, user.Username),
                    new(ClaimTypes.Role, user.Role),
                    new(StampClaim, user.SecurityStamp),
                },
                CookieAuthenticationDefaults.AuthenticationScheme));
    }
}
