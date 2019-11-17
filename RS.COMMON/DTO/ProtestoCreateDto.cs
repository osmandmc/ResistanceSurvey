using System;
using System.Collections.Generic;

namespace RS.COMMON.DTO
{
    public class ProtestoCreateDto
    {
        public List<int> ProtestoTypeIds { get; set; }
        public bool IsAgainstProduction { get; set; }
        public DateTime ProtestoStartDate { get; set; }
        public DateTime? ProtestoEndDate { get; set; }
        public List<int> ProtestoPlaceIds { get; set; }
       
        public int GenderId { get; set; }
        public List<int> InterventionTypeIds { get; set; }
        public int? CustodyCount { get; set; }
       
        public int EmployeeCountInProtesto { get; set; }
        public int EmployeeCountInProtestoId { get; set; }
    }
}