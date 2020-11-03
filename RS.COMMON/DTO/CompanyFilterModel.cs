using System;
using System.Collections.Generic;
using System.Text;

namespace RS.COMMON.DTO
{
    public class CompanyFilterModel: FilterModel
    {
        public int? CompanyId { get; set; }
        public int? MainCompanyId { get; set; }
    }
}
