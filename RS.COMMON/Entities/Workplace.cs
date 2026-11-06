using RS.COMMON.Entities.LookupEntity;

namespace RS.COMMON.Entities
{
    /// <summary>
    /// İşyeri (R37): the place where the workers worked (a factory, hospital, building site, mine), not a company. A case
    /// names one only when it is not the employer itself. Owner (“kime ait”): the organization the place belongs to,
    /// which suggests the case's Asıl işveren. Typed in a form: waits for approval.
    /// </summary>
    public class Workplace : LookupEntity.LookupEntity, IApprovable
    {
        public int? CityId { get; set; }
        public int? DistrictId { get; set; }
        public int? OwnerCompanyId { get; set; }
        public bool Approved { get; set; }
        public bool Deleted { get; set; }

        public City City { get; set; }
        public District District { get; set; }
        public Company OwnerCompany { get; set; }
    }

    /// <summary>
    /// How the employer works in this case (R37): Direct, or Subcontractor below its Asıl işveren
    /// (Resistance.MainCompanyId). Unknown: the news does not say, or not set (Genel and Dayanışma cases). The values 3
    /// (Yüklenici) and 4 (İşletmeci) were merged into Subcontractor on the researchers' feedback (2026-11-06): TOKİ is the
    /// Asıl işveren of the company building its project. Do not reuse them.
    /// </summary>
    public enum EmployerRole
    {
        Unknown = 0,
        Direct = 1,
        Subcontractor = 2,
    }
}
