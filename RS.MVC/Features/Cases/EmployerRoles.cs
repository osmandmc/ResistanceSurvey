using RS.COMMON.Entities;
using RS.MVC.Models;

namespace RS.MVC.Features.Cases
{
    /// <summary>
    /// How the employer works in an İşletme case (R37) and what the organization above it is called. Until the new form
    /// ships, the old one sends “Taşeron mu?” (IsOutsource) and passes back the role it loaded: that role counts while it
    /// agrees with the answer (Yüklenici stays Yüklenici under “Evet”); otherwise the answer decides, Evet = Taşeron,
    /// Hayır = Doğrudan. The new form sends both in agreement.
    /// </summary>
    public static class EmployerRoles
    {
        public static EmployerRole Effective(CaseInput c) =>
            c.EmployerRole is { } role && (HasAbove(role) == c.IsOutsource || role == EmployerRole.Unknown && !c.IsOutsource)
                ? role
                : c.IsOutsource ? EmployerRole.Subcontractor : EmployerRole.Direct;

        /// <summary>Taşeron, Yüklenici and İşletmeci name an organization above the employer (MainCompanyId).</summary>
        public static bool HasAbove(EmployerRole role) =>
            role is EmployerRole.Subcontractor or EmployerRole.Contractor or EmployerRole.Operator;

        /// <summary>The name shown for the organization above, by role.</summary>
        public static string AboveLabel(EmployerRole role) => role switch
        {
            EmployerRole.Subcontractor => "Asıl işveren",
            EmployerRole.Contractor => "İş sahibi",
            EmployerRole.Operator => "Varlığın sahibi",
            _ => null,
        };

        /// <summary>The message when the organization above is missing.</summary>
        public static string AboveMissing(EmployerRole role) => role switch
        {
            EmployerRole.Subcontractor => "Lütfen asıl işvereni seçiniz.",
            EmployerRole.Contractor => "Lütfen iş sahibini seçiniz.",
            EmployerRole.Operator => "Lütfen varlığın sahibini seçiniz.",
            _ => null,
        };

        public static string Name(EmployerRole role) => role switch
        {
            EmployerRole.Direct => "Doğrudan",
            EmployerRole.Subcontractor => "Taşeron",
            EmployerRole.Contractor => "Yüklenici",
            EmployerRole.Operator => "İşletmeci",
            _ => "Bilinmiyor",
        };
    }
}
