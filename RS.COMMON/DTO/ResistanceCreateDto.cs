using System;
using System.Collections.Generic;

namespace RS.COMMON.DTO
{
    public class ResistanceCreateDto
    {
        public int CompanyId { get; set; }
        public int CategoryId { get; set; }
        public bool HasTradeUnion { get; set; }
        public int? TradeUnionAuthorityId { get; set; }
        //public string DirectlyCommunicated { get; set; }
        public int EmployeeCountId { get; set; }
        public int EmployeeCount { get; set; }
        public List<int> CorporationIds { get; set; }
        public int? TradeUnionId { get; set; }
        public List<int> EmploymentTypeIds { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool AnyLegalIntervention { get; set; }
        public string LegalInterventionDesc { get; set; }
        public int? FiredEmployeeCountByProtesto { get; set; }
    }

     public class ResistanceDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int CategoryId { get; set; }
        public bool HasTradeUnion { get; set; }
        public int? TradeUnionAuthorityId { get; set; }
        public string DirectlyCommunicated { get; set; }
        public int EmployeeCountId { get; set; }
        public int EmployeeCount { get; set; }
        public List<int> CorporationIds { get; set; }
        public int? TradeUnionId { get; set; }
        public List<int> EmploymentTypeIds { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}