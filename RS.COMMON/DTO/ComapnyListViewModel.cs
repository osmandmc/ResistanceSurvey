using System;
using System.Collections.Generic;
using System.Text;

namespace RS.COMMON.DTO
{
    public class ComapnyListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int MainCompanyId { get; set; }
        public string MainCorporationName { get; set; }
        public string Scale { get; set; }
        public string WorkLine { get; set; }
        public string Type { get; set; }
        public bool IsOutsource => !String.IsNullOrEmpty(MainCorporationName);
    }
    public class CompanyDeleteModel
    {
        public int CompanyId { get; set; }
        public int MainCompanyId { get; set; }
    }
}
