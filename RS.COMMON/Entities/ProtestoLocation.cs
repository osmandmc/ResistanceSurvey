using RS.COMMON.Entities.LookupEntity;
using System;
using System.Collections.Generic;
using System.Text;

namespace RS.COMMON.Entities
{
    public class ProtestoLocation
    {
        public int Id { get; set; }
        public int ProtestoId { get; set; }
        public int CityId { get; set; }
        public int? DistrictId { get; set; }
        public string Place { get; set; }

        public Protesto Protesto { get; set; }
        public City City { get; set; }
        public District District { get; set; }
    }
}
