using System;
using System.Collections.Generic;

namespace RS.COMMON.DTO
{
    public class ResistanceDetailDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int CategoryId { get; set; }
        public bool HasTradeUnion { get; set; }
        public bool DevelopRight { get; set; }
        public int? TradeUnionId { get; set; }
        public int? TradeUnionAuthorityId { get; set; }
        public int EmployeeCountId { get; set; }
        public int EmployeeCount { get; set; }
        public bool AnyLegalIntervention { get; set; }
        public string LegalInterventionDesc { get; set; }
        public int? FiredEmployeeCountByProtesto { get; set; }
        public List<int> CorporationIds { get; set; }
        public List<int> EmploymentTypeIds { get; set; }
        public List<int> ResistanceReasonIds { get; set; }
        public List<int> ResistanceNewsIds { get; set; }
        public List<ProtestoListModel> Protestos { get; set; }
        public ResistanceDetailDto()
        {
            Protestos = new List<ProtestoListModel>();
            EmploymentTypeIds = new List<int>();
            CorporationIds = new List<int>();
            ResistanceReasonIds = new List<int>();
            ResistanceNewsIds = new List<int>();
        }

        public ResistanceDetailDto(int id, int companyId, int categoryId, bool hasTradeUnion, int? tradeUnionAuthorityId, int? tradeUnionId, int employeeCount, int employeeCountId)
        {
            Id = id;
            CompanyId = companyId;
            CategoryId = categoryId;
            HasTradeUnion = hasTradeUnion;
            TradeUnionAuthorityId = tradeUnionAuthorityId;
            TradeUnionId = tradeUnionId;
            EmployeeCount = employeeCount;
            EmployeeCountId = employeeCountId;
            
            Protestos = new List<ProtestoListModel>();
            EmploymentTypeIds = new List<int>();
            CorporationIds = new List<int>();
            ResistanceReasonIds = new List<int>();
            ResistanceNewsIds = new List<int>();
        }
    }
    public class ProtestoListModel
    {
        public int ProtestoId { get; set; }
        public List<string> ProtestoTypes { get; set; }
        public DateTime ProtestoStartDate { get; set; }
    }

    public class ResistanceQueryModel
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int CategoryId { get; set; }
        public bool HasTradeUnion { get; set; }
        public int? TradeUnionAuthorityId { get; set; }
        public int? TradeUnionId { get; set; }
        public int EmployeeCountId { get; set; }
        public int EmployeeCount { get; set; }
        public int CorporationId { get; set; }
        public int EmploymentTypeId { get; set; }
        public int ProtestoId { get; set; }
        public DateTime ProtestoStartDate { get; set; }
        public string ProtestoType { get; set; }
    }
    

}