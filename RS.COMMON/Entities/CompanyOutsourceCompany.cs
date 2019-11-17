using RS.COMMON.Entities.LookupEntity;

namespace RS.COMMON.Entities
{
    public class CompanyOutsourceCompany
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int OutsourceCompanyId { get; set; }

        public Company Company { get; set; }
        public Company OutsourceCompany { get; set; }
    }
}