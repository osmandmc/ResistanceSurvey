using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RS.COMMON.DTO
{
    public class CorporationListView
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? CorporationTypeId { get; set; }
        public string CorporationTypeName { get; set; }
        public bool Approved { get; set; }
    }
}
