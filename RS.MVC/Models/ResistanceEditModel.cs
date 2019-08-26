using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RS.COMMON.DTO;
using RS.COMMON.Entities;

namespace RS.MVC.Models
{
    public class ResistanceEditModel
    {
        public ResistanceEditModel (){}
        public int Id { get; set; }
        [Required(ErrorMessage = "Bu alan zorunludur.")]
        public string Code { get; set; }
        public int CategoryId { get; set; }
        [Required(ErrorMessage = "Bu alan zorunludur.")]
        public int CompanyId { get; set; }
        public int? OutsourceCompanyId { get; set; }
        public bool HasTradeUnion { get; set; }
        public int? TradeUnionAuthorityId { get; set; }

        public int? EmployeeCountId { get; set; }
        public int? EmployeeCount { get; set; }
       

        public List<int> CorporationIds { get; set; }
        public int? TradeUnionId { get; set; }
        public List<int> EmploymentTypeIds { get; set; }
        public List<ProtestoListModel> Protestos { get; set; }
        public ResistanceEditModel(ResistanceDetailDto dto)
        {
            
            CategoryId = dto.CategoryId;
            CompanyId = dto.CompanyId;
            CorporationIds = dto.CorporationIds;
            EmployeeCount = dto.EmployeeCount;
            EmployeeCountId = dto.EmployeeCountId;
            EmploymentTypeIds = dto.EmploymentTypeIds;
            TradeUnionId = dto.TradeUnionId;
            TradeUnionAuthorityId = dto.TradeUnionAuthorityId;
            HasTradeUnion = dto.HasTradeUnion;
            Protestos = dto.Protestos;
            EmployeeCount = dto.EmployeeCount;
            EmployeeCountId = dto.EmployeeCountId;
            
        } 
        

    }
}