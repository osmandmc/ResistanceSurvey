using System;

namespace RS.COMMON.Entities
{
    /// <summary>
    /// What the R31 migration changed on a case, written before the change: the old company, main company, "Hedef kurum"
    /// and "Kime karşı" values, the old name, and the placeholder organizers it removed. Nothing is lost: the changes can
    /// be checked or undone from it, and the Dayanışma linking screen groups old cases by OldCompanyName.
    /// </summary>
    public class CaseMigrationRecord
    {
        public int Id { get; set; }
        public int ResistanceId { get; set; }

        /// <summary>"general", "solidarity" or "workplace-target" (an İşletme case with a "Hedef kurum", to check by hand).</summary>
        public string Step { get; set; }

        public int? OldCompanyId { get; set; }
        public string OldCompanyName { get; set; }
        public int? OldMainCompanyId { get; set; }
        public int? OldCorporationId { get; set; }
        public int OldTargetType { get; set; }
        public string OldName { get; set; }

        /// <summary>Ids of the "Kurumsuz" / "Diğer" organizers removed from the case, comma separated.</summary>
        public string RemovedCorporationIds { get; set; }

        /// <summary>The migration linked OldCorporationId as an organizer (the case did not have it yet).</summary>
        public bool AddedOrganizingLink { get; set; }

        /// <summary>The migration marked OldCompanyId deleted (it was used only by Genel cases).</summary>
        public bool CompanyMarkedDeleted { get; set; }

        /// <summary>Set when a researcher has dealt with the case (R31c linking screen, or the İşletme check).</summary>
        public DateTime? ResolvedAt { get; set; }

        public DateTime At { get; set; }
    }
}
