using System;
using System.Collections.Generic;
using RS.COMMON.DTO;
using RS.COMMON.Entities.LookupEntity;
using static RS.COMMON.Constants.Enums;

namespace RS.COMMON.Entities
{
    public class Resistance: AuditEntity
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int? CompanyId { get; set; }
        public int? MainCompanyId { get; set; }
        public int? CorporationId { get; set; }
        public ResistanceTargetType ResistanceTargetType { get; set; }
        public string Code { get; set; }
        public int? EmployeeCountNumber { get; set; }
        public int? EmployeeCountId { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime StartDate { get; set; }
        public bool HasTradeUnion { get; set; }
        public bool DevelopRight { get; set; }
        public int? TradeUnionId { get; set; }
        public int? TradeUnionAuthorityId { get; set; }
        public string Note { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }

        public bool? AnyLegalIntervention { get; set; }
        public string LegalInterventionDesc { get; set; }
        public int? FiredEmployeeCountByProtesto { get; set; }
        public ResistanceResult ResistanceResult { get; set; }
        public bool Deleted { get; set; }

        public Category Category { get; set; }
        public Company Company { get; set; }
        public Company MainCompany { get; set; }
        public EmployeeCount EmployeeCount { get; set; }
        public Corporation Corporation { get; set; }
        public Corporation TradeUnion { get; set; }
        public TradeUnionAuthority TradeUnionAuthority { get; set; }
        public List<Protesto> Protestos { get; set; }
        public List<ResistanceCorporation> ResistanceCorporations { get; set; }
        public List<ResistanceEmploymentType> ResistanceEmploymentTypes { get; set; }
        public List<ResistanceResistanceReason> ResistanceResistanceReasons { get; set; }
        public List<ResistanceNews> ResistanceNews { get; set; }

        public Resistance(){}

        
    }

}

