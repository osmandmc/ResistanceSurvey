using System.Collections.Generic;
using RS.COMMON.Entities.LookupEntity;

namespace RS.COMMON.Entities
{
    /// <summary>
    /// An employer or other organization a case names (R37: a legal entity, not a place; places are Workplace).
    /// CompanyType is shown as “Sahiplik” (ownership: Kamu, Özel yerli, Özel yabancı, Karma …); CompanyWorkLine is the
    /// workers' işkolu and ServiceBranch the civil servants' hizmet kolu.
    /// </summary>
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? CompanyWorkLineId { get; set; }
        public int? CompanyScaleId { get; set; }
        public int? CompanyTypeId { get; set; }
        /// <summary>“Kurum türü” (R37): no longer asked (researchers, 2026-11-06: İstihdam türü already says it); the
        /// column keeps any values entered since 2026-11-01.</summary>
        public InstitutionKind? InstitutionKind { get; set; }
        public int? ServiceBranchId { get; set; }
        public bool Deleted { get; set; }
        public CompanyWorkLine CompanyWorkLine { get; set; }
        public CompanyWorkLine ServiceBranch { get; set; }
        public CompanyScale CompanyScale { get; set; }
        public CompanyType CompanyType { get; set; }
        public ICollection<CompanyOutsourceCompany> OutsourceCompanies { get; set; }
    }

    /// <summary>“Kurum türü” (R37): what the organization is, independent of who owns it.</summary>
    public enum InstitutionKind
    {
        Company = 1,
        Holding = 2,
        PublicBody = 3,
        StateEnterprise = 4,
        Municipality = 5,
        MunicipalCompany = 6,
        Cooperative = 7,
        Other = 8,
    }
}
