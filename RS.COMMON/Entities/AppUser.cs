using System;

namespace RS.COMMON.Entities
{
    /// <summary>
    /// A person's account (F4a). A yönetici invites someone with a one-time link; the person sets their own password
    /// through it. Only hashes are stored: of the password and of the link's token.
    /// </summary>
    public class AppUser
    {
        public int Id { get; set; }

        /// <summary>Signs in with it; unique, compared by Turkish rules (case ignored).</summary>
        public string Username { get; set; }

        /// <summary><see cref="Constants.Role.Admin"/> (yönetici) or <see cref="Constants.Role.User"/> (araştırmacı).</summary>
        public string Role { get; set; }

        /// <summary>Null until the person has set a password through their link.</summary>
        public string PasswordHash { get; set; }

        /// <summary>SHA-256 (hex) of the open link's token; null when no link is open.</summary>
        public string InviteTokenHash { get; set; }
        public DateTime? InviteExpiresAt { get; set; }

        /// <summary>Wrong passwords in a row; at the limit sign-in is refused until <see cref="LockedUntil"/>.</summary>
        public int FailedSignIns { get; set; }
        public DateTime? LockedUntil { get; set; }

        /// <summary>Cannot sign in; open sessions end at their next request.</summary>
        public bool Disabled { get; set; }

        /// <summary>Changes when the password is set or the account is disabled: cookies issued before stop working.</summary>
        public string SecurityStamp { get; set; }

        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastSignInAt { get; set; }
    }
}
