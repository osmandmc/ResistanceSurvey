using System;
using System.Collections.Generic;
using RS.COMMON.Entities;

namespace RS.COMMON.DTO
{
    public class ProtestoAddModel
    {
        public int ResistanceId { get; set; }
        public List<int> ProtestoTypeIds { get; set; }
        public bool IsAgainstProduction { get; set; }
        public DateTime ProtestoStartDate { get; set; }
        public DateTime? ProtestoEndDate { get; set; }
        public List<int> ProtestoPlaceIds { get; set; }
        public int GenderId { get; set; }
        public List<int> InterventionTypeIds { get; set; }
        public int? CustodyCount { get; set; }
        public int? EmployeeCountInProtesto { get; set; }
        public int? EmployeeCountInProtestoId { get; set; }
        public bool DevelopRight { get; set; }
      
        public string Note { get; set; }
        public Protesto ToEntity()
        {
            var protesto = new Protesto
            {
                ResistanceId = ResistanceId,
                ProtestoEmployeeCountId = EmployeeCountInProtestoId,
                EmployeeCountNumber = EmployeeCountInProtesto,
                StartDate = ProtestoStartDate,
                EndDate = ProtestoEndDate,
                ProtestoProtestoTypes = new List<ProtestoProtestoType>(),
                ProtestoProtestoPlaces = new List<ProtestoProtestoPlace>(),
                ProtestoInterventionTypes = new List<ProtestoInterventionType>(),
                GenderId = GenderId,
                CustodyCount = CustodyCount
            };
            if(ProtestoPlaceIds!=null)
                ProtestoPlaceIds.ForEach(c=> protesto.ProtestoProtestoPlaces.Add(new ProtestoProtestoPlace{ ProtestoPlaceId = c }));
            if(ProtestoTypeIds!=null)
                ProtestoTypeIds.ForEach(c=> protesto.ProtestoProtestoTypes.Add(new ProtestoProtestoType{ ProtestoTypeId = c }));
            if(InterventionTypeIds!=null)
                InterventionTypeIds.ForEach(c=> protesto.ProtestoInterventionTypes.Add(new ProtestoInterventionType{ InterventionTypeId = c}));
            return protesto;
        }
    }
}