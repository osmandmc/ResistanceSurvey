using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using RS.COMMON.DTO;
using RS.COMMON.Entities;
using RS.COMMON.Entities.LookupEntity;
using static RS.COMMON.Constants.Enums;

namespace RS.MVC.Models
{
    public class ResistanceCreateModel: BaseViewModel
    {
        [Required(ErrorMessage = "Bu alan zorunludur.")]
        public int CategoryId { get; set; }
        [Required(ErrorMessage = "Bu alan zorunludur.")]
        public int CompanyId { get; set; }
        public int? MainCompanyId { get; set; }
        public string Code { get; set; }
        public bool HasTradeUnion { get; set; }
        public int? TradeUnionAuthorityId { get; set; }
        public string Note { get; set; }
        public string ResistanceDescription { get; set; }

        public int? EmployeeCountId { get; set; }
        public int? EmployeeCount { get; set; }
        public int? EmployeeCountInProtestoId { get; set; }

        public int? EmployeeCountInProtesto { get; set; }

        public List<string> CorporationIds { get; set; }
        public int? TradeUnionId { get; set; }
        public List<int> EmploymentTypeIds { get; set; }
        public List<string> ResistanceReasonIds { get; set; }
        public int CompanyTypeId { get; set; }
        public int CompanyScaleId { get; set; }

        [Required(ErrorMessage = "Bu alan zorunludur.")]
        public List<string> ProtestoTypeIds { get; set; }
        public bool DevelopRight { get; set; }
        public DateTime ProtestoStartDate { get; set; }
        public DateTime? ProtestoEndDate { get; set; }
        [Required(ErrorMessage = "Bu alan zorunludur.")]
        public List<string> ProtestoPlaceIds { get; set; }
        public int GenderId { get; set; }
        public List<int> InterventionTypeIds { get; set; }
        public short AnyLegalIntervention { get; set; }
        public string LegalIntervantionDesc { get; set; }
        public int? CustodyCount { get; set; }
        public int? FiredEmployeeCountByProtesto { get; set; }
        public ResistanceResult ResistanceResult { get; set; }
        public string SimpleProtestoDescription { get; set; }
        public int StrikeDuration { get; set; }

        //public List<int> ResistanceNewsIds { get; set; }
        public List<int> ProtestoCityIds { get; set; }
        public List<int?> ProtestoDistrictIds { get; set; }
        public List<ProtestoLocationModel> Locations { get; set; }
        public List<ResistanceNewsModel> ResistanceNewsIds { get; set; }
        public Resistance MapToResistanceDto()
        {
            var resistance = new Resistance
            {
                CompanyId = CompanyId,
                MainCompanyId = MainCompanyId,
                CategoryId = CategoryId,
                Code = Code,
                HasTradeUnion = HasTradeUnion,
                DevelopRight = DevelopRight,
                TradeUnionId = TradeUnionId,
                TradeUnionAuthorityId = TradeUnionAuthorityId,
                EmployeeCountId = EmployeeCountId,
                EmployeeCountNumber = EmployeeCount,
                StartDate = ProtestoStartDate,
                EndDate = ProtestoEndDate,
                Description = ResistanceDescription,
                Note = Note,
                FiredEmployeeCountByProtesto = FiredEmployeeCountByProtesto,
                Creator = UserName,
                CreateDate = DateTime.Now,
                LegalInterventionDesc = LegalIntervantionDesc,
                ResistanceResult = ResistanceResult,
                ResistanceCorporations = new List<ResistanceCorporation>(),
                ResistanceEmploymentTypes = new List<ResistanceEmploymentType>(),
                ResistanceResistanceReasons = new List<ResistanceResistanceReason>(),
                ResistanceNews = new List<ResistanceNews>(),
            };
            if (AnyLegalIntervention == 1) resistance.AnyLegalIntervention = true;
            if (AnyLegalIntervention == 2) resistance.AnyLegalIntervention = false;
            if (EmploymentTypeIds != null)
                EmploymentTypeIds.ForEach(c=> resistance.ResistanceEmploymentTypes.Add(new ResistanceEmploymentType{EmploymentTypeId = c, ResistanceId = resistance.Id}));
            
            //if(ResistanceNewsIds != null)
            //    ResistanceNewsIds.ForEach(c=> resistance.ResistanceNews.Add(new ResistanceNews{NewsId = c, ResistanceId = resistance.Id}));
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
                SimpleProtestoDescription = SimpleProtestoDescription,
                StrikeDuration = StrikeDuration,
                Creator = UserName,
                CreateDate = DateTime.Now,
                ProtestoProtestoTypes = new List<ProtestoProtestoType>(),
                ProtestoProtestoPlaces = new List<ProtestoProtestoPlace>(),
                ProtestoInterventionTypes = new List<ProtestoInterventionType>(),
                Cities = new List<ProtestoCity>(),
                Districts = new List<ProtestoDistrict>(),
                Locations = new List<ProtestoLocation>()
            };
            if(InterventionTypeIds!=null)
                InterventionTypeIds.ForEach(c=> protesto.ProtestoInterventionTypes.Add(new ProtestoInterventionType{ InterventionTypeId = c}));
            if(ProtestoCityIds!=null)
                ProtestoCityIds.ForEach(c=> protesto.Cities.Add(new ProtestoCity{ CityId = c}));
            if(ProtestoDistrictIds!=null)
                ProtestoDistrictIds.ForEach(c=> {if(c.HasValue) protesto.Districts.Add(new ProtestoDistrict{ DistrictId = c.Value});} );
            if (Locations != null)
                Locations.Where(s => !s.Deleted).ToList().ForEach(c => { protesto.Locations.Add(new ProtestoLocation { CityId = c.CityId.Value, DistrictId = c.DistrictId, Place = c.Place }); });
            return protesto;
        }
    }
    
    public class ResistanceModel: BaseViewModel
    {
        [Required(ErrorMessage = "Bu alan zorunludur.")]
        public int CategoryId { get; set; }
        [Required(ErrorMessage = "Bu alan zorunludur.")]
        public int CompanyId { get; set; }
        public int? MainCompanyId { get; set; }
        public string Code { get; set; }
        public bool HasTradeUnion { get; set; }
        public int? TradeUnionAuthorityId { get; set; }
        public string Note { get; set; }
        public string ResistanceDescription { get; set; }
        public int? EmployeeCountId { get; set; }
        public int? EmployeeCount { get; set; }
        public List<Corporation> CorporationIds { get; set; }
        public int? TradeUnionId { get; set; }
        public List<EmploymentType> EmploymentTypeIds { get; set; }
        public List<ResistanceReason> ResistanceReasonIds { get; set; }
        public ProtestoModel Protesto { get; set; }
        public List<ResistanceNewsModel> ResistanceNewsIds { get; set; }
        
        public Resistance MapToResistance()
        {
            var resistance = new Resistance
            {
                CompanyId = CompanyId,
                MainCompanyId = MainCompanyId,
                CategoryId = CategoryId,
                Code = Code,
                HasTradeUnion = HasTradeUnion,
                //DevelopRight = DevelopRight,
                TradeUnionId = TradeUnionId,
                TradeUnionAuthorityId = TradeUnionAuthorityId,
                EmployeeCountId = EmployeeCountId,
                EmployeeCountNumber = EmployeeCount,
                StartDate = Protesto.ProtestoStartDate,
                EndDate = Protesto.ProtestoEndDate,
                Description = ResistanceDescription,
                Note = Note,
                //FiredEmployeeCountByProtesto = Protesto.FiredEmployeeCountByProtesto,
                Creator = UserName,
                CreateDate = DateTime.Now,
                //LegalInterventionDesc = LegalIntervantionDesc,
                //ResistanceResult = ResistanceResult,
                ResistanceCorporations = new List<ResistanceCorporation>(),
                ResistanceEmploymentTypes = new List<ResistanceEmploymentType>(),
                ResistanceResistanceReasons = new List<ResistanceResistanceReason>(),
                ResistanceNews = new List<ResistanceNews>(),
                Protestos = new List<Protesto>(),
            };
            // if (AnyLegalIntervention == 1) resistance.AnyLegalIntervention = true;
            // if (AnyLegalIntervention == 2) resistance.AnyLegalIntervention = false;
            if (EmploymentTypeIds != null)
                EmploymentTypeIds.ForEach(c=> 
                        resistance.ResistanceEmploymentTypes
                            .AddRange(new ResistanceEmploymentType
                            {
                                EmploymentTypeId = c.Id, ResistanceId = resistance.Id
                            }));
            
            //if(ResistanceNewsIds != null)
            //    ResistanceNewsIds.ForEach(c=> resistance.ResistanceNews.Add(new ResistanceNews{NewsId = c, ResistanceId = resistance.Id}));

           
            return resistance;
        }
        public Protesto MapToProtestoDto()
        {
            var protesto = Protesto.ToEntity();
            return protesto;
        }
    }
}
