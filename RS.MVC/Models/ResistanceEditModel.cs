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
        public string Code { get; set; }
        public int CategoryId { get; set; }
        [Required(ErrorMessage = "Bu alan zorunludur.")]
        public int CompanyId { get; set; }
        public int? OutsourceCompanyId { get; set; }
        public bool IsOutsource { get; set; }
        public bool HasTradeUnion { get; set; }
        public bool DevelopRight { get; set; }
        public int? TradeUnionAuthorityId { get; set; }
        public string ResistanceDescription { get; set; } 

        public int? EmployeeCountId { get; set; }
        public int? EmployeeCount { get; set; }
        public string ResistanceNote { get; set; }
        public List<int> ResistanceReasonIds { get; set; }
        public List<int> CorporationIds { get; set; }
        public string NewCorporation { get; set; }
        public int? TradeUnionId { get; set; }
        public bool AnyLegalIntervention { get; set; }
        public string LegalInterventionDesc { get; set; }
        public int? FiredEmployeeCountByProtesto { get; set; }
        
        public List<int> EmploymentTypeIds { get; set; }
        public List<ResistanceNewsModel> ResistanceNewsIds { get; set; }
        
        public List<ProtestoListModel> Protestos { get; set; }
        public List<News> ResistanceNews { get; set; }
        
        

    }
}