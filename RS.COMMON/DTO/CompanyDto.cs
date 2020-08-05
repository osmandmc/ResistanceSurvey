using RS.COMMON.Entities;
using System.Collections.Generic;

namespace RS.COMMON.DTO
{
    public class CompanyDto
    {
        public string Name { get; set; }
        public int TypeId { get; set; }
        public int ScaleId { get; set; }
        public int WorklineId { get; set; }
        public int? OutsourceEmployerMainCompanyId { get; set; }
    }
   
    public class CompanyReturnDto
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public CompanyReturnDto(Company company)
        {
            CompanyId = company.Id;
            CompanyName = company.Name;
        }
       
    }
    public class OutsourceCompanyReturnDto{
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public int? MainCompanyId { get; set; }

        public OutsourceCompanyReturnDto(Company company, int? mainCompanyId)
        {
            CompanyId = company.Id;
            CompanyName = company.Name;
            MainCompanyId = mainCompanyId;
        }
    }
}