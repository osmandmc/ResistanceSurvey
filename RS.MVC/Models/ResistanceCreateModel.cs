using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RS.COMMON.DTO;
using RS.COMMON.Entities;

namespace RS.MVC.Models
{
    public class ResistanceCreateModel
    {
        [Required(ErrorMessage = "Bu alan zorunludur.")]
        public int CategoryId { get; set; }
        [Required(ErrorMessage = "Bu alan zorunludur.")]
        public int CompanyId { get; set; }
        public string Code { get; set; }
        public int? OutsourceCompanyId { get; set; }
        public bool HasTradeUnion { get; set; }
        public int? TradeUnionAuthorityId { get; set; }
        public string DirectlyCommunicated { get; set; }

        public int? EmployeeCountId { get; set; }
        public int? EmployeeCount { get; set; }
        public int? EmployeeCountInProtestoId { get; set; }

        public int? EmployeeCountInProtesto { get; set; }

        public List<int> CorporationIds { get; set; }
        public int? TradeUnionId { get; set; }
        public List<int> EmploymentTypeIds { get; set; }
        public int CompanyTypeId { get; set; }
        public int CompanyScaleId { get; set; }

        [Required(ErrorMessage = "Bu alan zorunludur.")]
        public List<int> ProtestoTypeIds { get; set; }
        public bool IsAgainstProduction { get; set; }
        public bool DevelopRight { get; set; }
        public DateTime ProtestoStartDate { get; set; }
        public DateTime? ProtestoEndDate { get; set; }
        [Required(ErrorMessage = "Bu alan zorunludur.")]
        public List<int> ProtestoPlaceIds { get; set; }
        public List<int> ProtestoReasonIds { get; set; }
        public int GenderId { get; set; }
        public List<int> InterventionTypeIds { get; set; }
        public bool AnyLegalIntervention { get; set; }
        public int? CustodyCount { get; set; }

        public Resistance MapToResistanceDto()
        {
            var resistance = new Resistance
            {
                CompanyId = CompanyId,
                CategoryId = CategoryId,
                Code = Code,
                HasTradeUnion = HasTradeUnion,
                TradeUnionAuthorityId = TradeUnionAuthorityId,
                TradeUnionId = TradeUnionId,
                EmployeeCountId = EmployeeCountId,
                EmployeeCountNumber = EmployeeCount,
                StartDate = ProtestoStartDate,
                EndDate = ProtestoEndDate,
                ResistanceCorporations = new List<ResistanceCorporation>(),
                ResistanceEmploymentTypes = new List<ResistanceEmploymentType>()
            };
            if(CorporationIds!= null)
                CorporationIds.ForEach(c=> resistance.ResistanceCorporations.Add(new ResistanceCorporation{CorporationId = c, ResistanceId = resistance.Id}));
            if(EmploymentTypeIds != null)
                EmploymentTypeIds.ForEach(c=> resistance.ResistanceEmploymentTypes.Add(new ResistanceEmploymentType{EmploymentTypeId = c, ResistanceId = resistance.Id}));
            return resistance;
        }
         public Protesto MapToProtestoDto()
        {
            var protesto = new Protesto
            {
                ProtestoEmployeeCountId = EmployeeCountInProtestoId,
                EmployeeCountNumber = EmployeeCountInProtesto,
                IsAgainstProduction = IsAgainstProduction,
                StartDate = ProtestoStartDate,
                EndDate = ProtestoEndDate,
                ProtestoProtestoResaons = new List<ProtestoProtestoReason>(),
                ProtestoProtestoTypes = new List<ProtestoProtestoType>(),
                ProtestoProtestoPlaces = new List<ProtestoProtestoPlace>(),
                ProtestoInterventionTypes = new List<ProtestoInterventionType>(),
                GenderId = GenderId,
                CustodyCount = CustodyCount
            };
            if(ProtestoPlaceIds!=null)
                ProtestoPlaceIds.ForEach(c=> protesto.ProtestoProtestoPlaces.Add(new ProtestoProtestoPlace{ ProtestoPlaceId = c }));
            if(ProtestoReasonIds!=null)
                ProtestoReasonIds.ForEach(c=> protesto.ProtestoProtestoResaons.Add(new ProtestoProtestoReason{ ProtestoReasonId = c }));
            if(ProtestoTypeIds!=null)
                ProtestoTypeIds.ForEach(c=> protesto.ProtestoProtestoTypes.Add(new ProtestoProtestoType{ ProtestoTypeId = c }));
            if(InterventionTypeIds!=null)
                InterventionTypeIds.ForEach(c=> protesto.ProtestoInterventionTypes.Add(new ProtestoInterventionType{ InterventionTypeId = c}));
            return protesto;
        }
        


    }
}