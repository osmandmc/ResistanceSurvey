using System;

namespace RS.COMMON.Entities
{
    /// <summary>
    /// A fact about two companies that holds whatever the case (R37, part B), read “Company → RelatedCompany”: Company is
    /// part of the group RelatedCompany (with a share), belongs to the public body RelatedCompany, or was renamed to
    /// RelatedCompany. Dated and sourced. Links that depend on the job (subcontracting, a contract, running a site) are
    /// on the case (Resistance.EmployerRole and MainCompanyId), not here.
    /// </summary>
    public class CompanyRelation
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int RelatedCompanyId { get; set; }
        public CompanyRelationKind Kind { get; set; }
        public decimal? SharePercent { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SourceUrl { get; set; }
        public string Note { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        public Company Company { get; set; }
        public Company RelatedCompany { get; set; }
    }

    public enum CompanyRelationKind
    {
        /// <summary>Company is part of the group (holding) RelatedCompany.</summary>
        Group = 1,

        /// <summary>Company belongs to the public body RelatedCompany (a hospital to a ministry, a municipal company to its municipality).</summary>
        PublicBody = 2,

        /// <summary>Company was renamed to RelatedCompany (StartDate: when).</summary>
        Renamed = 3,
    }
}
