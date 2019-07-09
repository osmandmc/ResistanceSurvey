using System;
using System.Collections.Generic;
using RS.COMMON.Entities.LookupEntity;

namespace RS.COMMON.Entities
{
    public class Resistance
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int CompanyId { get; set; }
        public string Code { get; set; }
        public string DirectlyCommunicated { get; set; }
        public int EmployeeCountNumber { get; set; }
        public int EmployeeCountId { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime StartDate { get; set; }
        public bool HasTradeUnion { get; set; }
        public int? TradeUnionId { get; set; }
        public int? TradeUnionAuthorityId { get; set; }

        public Category Category { get; set; }
        public Company Company { get; set; }
        public EmployeeCount EmployeeCount { get; set; }
        public TradeUnion TradeUnion { get; set; }
        public TradeUnionAuthority TradeUnionAuthority { get; set; }
        public List<ResistanceCorporation> ResistanceCorporations { get; set; }
        public List<ResistanceEmploymentType> ResistanceEmploymentTypes { get; set; }
    }

}

