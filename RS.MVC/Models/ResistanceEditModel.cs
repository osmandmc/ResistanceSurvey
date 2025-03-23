using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RS.COMMON.DTO;
using RS.COMMON.Entities;
using RS.COMMON.Entities.LookupEntity;
using static RS.COMMON.Constants.Enums;

namespace RS.MVC.Models
{
    public class ResistanceEditModel: BaseViewModel
    {
        public ResistanceEditModel (){}
        public int Id { get; set; }
        public string Code { get; set; }
        public int CategoryId { get; set; }
        [Required(ErrorMessage = "Bu alan zorunludur.")]
        public Company CompanyId { get; set; }
        public Company MainCompanyId { get; set; }
        public bool IsOutsource { get; set; }
        public bool HasTradeUnion { get; set; }
        public bool DevelopRight { get; set; }
        public int? TradeUnionAuthorityId { get; set; }
        public string ResistanceDescription { get; set; } 

        public int? EmployeeCountId { get; set; }
        public int? EmployeeCount { get; set; }
        public string ResistanceNote { get; set; }
        public List<ResistanceReason> ResistanceReasonIds { get; set; }
        public List<Corporation> CorporationIds { get; set; }
        public int? TradeUnionId { get; set; }
        public short AnyLegalIntervention { get; set; }
        public string LegalInterventionDesc { get; set; }
        public int? FiredEmployeeCountByProtesto { get; set; }
        public ResistanceResult ResistanceResult { get; set; }
        public List<EmploymentType> EmploymentTypeIds { get; set; }
        public List<News> ResistanceNews { get; set; }
        public List<ProtestoListModel> Protestos { get; set; }
        public List<ProtestoEditModel> ProtestoItems { get; set; }
    }
}