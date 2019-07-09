using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RS.COMMON.DTO;

namespace RS.MVC.Models
{
    public class ResistanceCreateModel
    {
        [Required(ErrorMessage = "Bu alan zorunludur.")]
        public int CategoryId { get; set; }
        [Required(ErrorMessage = "Bu alan zorunludur.")]
        public int CompanyId { get; set; }
        public int? OutsourceCompanyId { get; set; }
        public bool HasTradeUnion { get; set; }
        public int? TradeUnionAuthorityId { get; set; }
        public string DirectlyCommunicated { get; set; }

        public int EmployeeCountId { get; set; }
        public int EmployeeCount { get; set; }
        public int EmployeeCountInProtestoId { get; set; }

        public int EmployeeCountInProtesto { get; set; }

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

        public ResistanceCreateDto MapToResistanceDto()
        {
            return new ResistanceCreateDto
            {
                CompanyId = CompanyId,
                CategoryId = CategoryId,
                CorporationIds = CorporationIds,
                HasTradeUnion = HasTradeUnion,
                TradeUnionAuthorityId = TradeUnionAuthorityId,
                TradeUnionId = TradeUnionId,
                EmployeeCountId = EmployeeCountId,
                StartDate = ProtestoStartDate,
                EndDate = ProtestoEndDate,
                EmploymentTypeIds = EmploymentTypeIds
            };
           
        }
         public ProtestoCreateDto MapToProtestoDto()
        {
            return new ProtestoCreateDto
            {
                EmployeeCountInProtestoId = EmployeeCountInProtestoId,
                EmployeeCountInProtesto = EmployeeCountInProtesto,
                IsAgainstProduction = IsAgainstProduction,
                ProtestoStartDate = ProtestoStartDate,
                ProtestoEndDate = ProtestoEndDate,
                ProtestoPlaceIds = ProtestoPlaceIds,
                ProtestoReasonIds = ProtestoReasonIds,
                ProtestoTypeIds = ProtestoTypeIds,
                GenderId = GenderId,
                InterventionTypeIds = InterventionTypeIds,
                CustodyCount = CustodyCount
            };
           
        }
        


    }
}