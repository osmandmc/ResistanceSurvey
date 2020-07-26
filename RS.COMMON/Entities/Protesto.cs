using System;
using System.Collections.Generic;
using RS.COMMON.Entities.LookupEntity;

namespace RS.COMMON.Entities
{
    public class Protesto: AuditEntity
    {
        public int Id { get; set; }
        public int ResistanceId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? EmployeeCountNumber { get; set; }
        public int? ProtestoEmployeeCountId { get; set; }
        public int GenderId { get; set; }
        public int? CustodyCount { get; set; }
       
        public string Note { get; set; }


        public Resistance Resistance { get; set; }
        public ProtestoEmployeeCount ProtestoEmployeeCount { get; set; }
        public List<ProtestoCity> Cities { get; set; }
        public List<ProtestoDistrict> Districts { get; set; }
        public List<ProtestoInterventionType> ProtestoInterventionTypes { get; set; }
        public ICollection<ProtestoProtestoPlace> ProtestoProtestoPlaces { get; set; }
        public ICollection<ProtestoProtestoType> ProtestoProtestoTypes { get; set; }

    }
}