using System.Collections.Generic;
using RS.COMMON.Entities.LookupEntity;

namespace RS.COMMON.Entities
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsOutsource { get; set; }
        public int? CompanyWorkLineId { get; set; }
        public int? CompanyScaleId { get; set; }
        public int? CompanyTypeId { get; set; }
        public bool Deleted { get; set; }

        public CompanyWorkLine CompanyWorkLine { get; set; }
        public CompanyScale CompanyScale { get; set; }
        public CompanyType CompanyType { get; set; }
        public ICollection<CompanyOutsourceCompany> OutsourceCompanies { get; set; }
    }
}