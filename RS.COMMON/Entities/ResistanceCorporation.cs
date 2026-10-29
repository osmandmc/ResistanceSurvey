using RS.COMMON.Entities.LookupEntity;

namespace RS.COMMON.Entities
{
    /// <summary>An organization on a case, in a role (R31).</summary>
    public class ResistanceCorporation
    {
        public int Id { get; set; }
        public int ResistanceId { get; set; }
        public int CorporationId { get; set; }

        /// <summary>Supporting (İşletme cases: "Destekleyen kurumlar") or Organizing (Genel and Dayanışma: "Örgütleyen kurumlar").</summary>
        public OrganizationRole Role { get; set; } = OrganizationRole.Supporting;

        public Resistance Resistance { get; set; }
        public Corporation Corporation { get; set; }
    }

    public enum OrganizationRole
    {
        Supporting = 1,
        Organizing = 2,
    }
}