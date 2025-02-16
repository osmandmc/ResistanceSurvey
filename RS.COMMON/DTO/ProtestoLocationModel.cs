using System;
using System.Collections.Generic;
using System.Text;

namespace RS.COMMON.DTO
{
    public class ProtestoLocationModel
    {
        public int Id { get; set; }
        public int ProtestoId { get; set; }
        public int? CityId { get; set; }
        public int? DistrictId { get; set; }
        public string Place { get; set; }
       
        public bool Deleted { get; set; }
    }
}

