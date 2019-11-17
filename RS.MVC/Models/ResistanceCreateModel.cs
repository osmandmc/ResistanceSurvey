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
        public string Note { get; set; }
        public string ResistanceDescription { get; set; }

        public int? EmployeeCountId { get; set; }
        public int? EmployeeCount { get; set; }
        public int? EmployeeCountInProtestoId { get; set; }

        public int? EmployeeCountInProtesto { get; set; }

        public List<int> CorporationIds { get; set; }
        public int? TradeUnionId { get; set; }
        public List<int> EmploymentTypeIds { get; set; }
        public List<int> ResistanceReasonIds { get; set; }
        public int CompanyTypeId { get; set; }
        public int CompanyScaleId { get; set; }

        [Required(ErrorMessage = "Bu alan zorunludur.")]
        public List<int> ProtestoTypeIds { get; set; }
        public bool DevelopRight { get; set; }
        public DateTime ProtestoStartDate { get; set; }
        public DateTime? ProtestoEndDate { get; set; }
        [Required(ErrorMessage = "Bu alan zorunludur.")]
        public List<int> ProtestoPlaceIds { get; set; }
        public int GenderId { get; set; }
        public List<int> InterventionTypeIds { get; set; }
        public bool AnyLegalIntervention { get; set; }
        public string LegalIntervantionDesc { get; set; }
        public int? CustodyCount { get; set; }
        public int? FiredEmployeeCountByProtesto { get; set; }
        public List<int> ResistanceNewsIds { get; set; }
        public List<int> ProtestoCityIds { get; set; }
        public List<int?> ProtestoDistrictIds { get; set; }

        public Resistance MapToResistanceDto()
        {
            var resistance = new Resistance
            {
                CompanyId = CompanyId,
                CategoryId = CategoryId,
                Code = Code,
                HasTradeUnion = HasTradeUnion,
                DevelopRight = DevelopRight,
                TradeUnionAuthorityId = TradeUnionAuthorityId,
                TradeUnionId = TradeUnionId,
                EmployeeCountId = EmployeeCountId,
                EmployeeCountNumber = EmployeeCount,
                StartDate = ProtestoStartDate,
                EndDate = ProtestoEndDate,
                Description = ResistanceDescription,
                Note = Note,
                FiredEmployeeCountByProtesto = FiredEmployeeCountByProtesto,
                AnyLegalIntervention = AnyLegalIntervention,
                LegalInterventionDesc = LegalIntervantionDesc,
                ResistanceCorporations = new List<ResistanceCorporation>(),
                ResistanceEmploymentTypes = new List<ResistanceEmploymentType>(),
                ResistanceResistanceReasons = new List<ResistanceResistanceReason>(),
                ResistanceNews = new List<ResistanceNews>()
                
            };
            if(CorporationIds!= null)
                CorporationIds.ForEach(c=> resistance.ResistanceCorporations.Add(new ResistanceCorporation{CorporationId = c, ResistanceId = resistance.Id}));
            if(EmploymentTypeIds != null)
                EmploymentTypeIds.ForEach(c=> resistance.ResistanceEmploymentTypes.Add(new ResistanceEmploymentType{EmploymentTypeId = c, ResistanceId = resistance.Id}));
            if(ResistanceReasonIds != null)
                ResistanceReasonIds.ForEach(c=> resistance.ResistanceResistanceReasons.Add(new ResistanceResistanceReason{ResistanceReasonId = c, ResistanceId = resistance.Id}));
            if(ResistanceNewsIds != null)
                ResistanceNewsIds.ForEach(c=> resistance.ResistanceNews.Add(new ResistanceNews{NewsId = c, ResistanceId = resistance.Id}));
            return resistance;
        }
        public Protesto MapToProtestoDto()
        {
            var protesto = new Protesto
            {
                ProtestoEmployeeCountId = EmployeeCountInProtestoId,
                EmployeeCountNumber = EmployeeCountInProtesto,
               
                StartDate = ProtestoStartDate,
                EndDate = ProtestoEndDate,
                GenderId = GenderId,
                CustodyCount = CustodyCount,
               
                ProtestoProtestoTypes = new List<ProtestoProtestoType>(),
                ProtestoProtestoPlaces = new List<ProtestoProtestoPlace>(),
                ProtestoInterventionTypes = new List<ProtestoInterventionType>(),
                Cities = new List<ProtestoCity>(),
                Districts = new List<ProtestoDistrict>()  
            };
            if(ProtestoPlaceIds!=null)
                ProtestoPlaceIds.ForEach(c=> protesto.ProtestoProtestoPlaces.Add(new ProtestoProtestoPlace{ ProtestoPlaceId = c }));
            if(ProtestoTypeIds!=null)
                ProtestoTypeIds.ForEach(c=> protesto.ProtestoProtestoTypes.Add(new ProtestoProtestoType{ ProtestoTypeId = c }));
            if(InterventionTypeIds!=null)
                InterventionTypeIds.ForEach(c=> protesto.ProtestoInterventionTypes.Add(new ProtestoInterventionType{ InterventionTypeId = c}));
            if(ProtestoCityIds!=null)
                ProtestoCityIds.ForEach(c=> protesto.Cities.Add(new ProtestoCity{ CityId = c}));
            if(ProtestoDistrictIds!=null)
                ProtestoDistrictIds.ForEach(c=> {if(c.HasValue) protesto.Districts.Add(new ProtestoDistrict{ DistrictId = c.Value});} );
            return protesto;
        }
    }
}