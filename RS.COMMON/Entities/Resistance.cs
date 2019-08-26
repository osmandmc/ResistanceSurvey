using System;
using System.Collections.Generic;
using RS.COMMON.DTO;
using RS.COMMON.Entities.LookupEntity;

namespace RS.COMMON.Entities
{
    public class Resistance
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int CompanyId { get; set; }
        public string Code { get; set; }
        public int? EmployeeCountNumber { get; set; }
        public int? EmployeeCountId { get; set; }
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
        public List<Protesto> Protestos { get; set; }
        public List<ResistanceCorporation> ResistanceCorporations { get; set; }
        public List<ResistanceEmploymentType> ResistanceEmploymentTypes { get; set; }
        public Resistance(){}
        public Resistance(ResistanceDetailDto dto)
        {
            Id = dto.Id;
            CompanyId = dto.CompanyId;
            CategoryId = dto.CategoryId;
            TradeUnionAuthorityId = dto.TradeUnionAuthorityId;
            TradeUnionId = dto.TradeUnionId;
            HasTradeUnion = dto.HasTradeUnion;
            EmployeeCountId = dto.EmployeeCountId;
            dto.EmploymentTypeIds.ForEach(emp => ResistanceEmploymentTypes.Add(new ResistanceEmploymentType{EmploymentTypeId = emp}));
            dto.CorporationIds.ForEach(corp => ResistanceCorporations.Add(new ResistanceCorporation{CorporationId = corp}));
        }
    }

}

