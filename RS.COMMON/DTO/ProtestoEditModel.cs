using System;
using System.Collections.Generic;
using RS.COMMON.Entities;

namespace RS.COMMON.DTO
{
    public class ProtestoEditModel
    {
        public int Id { get; set; }
        public int ResistanceId { get; set; }
        public List<int> ProtestoTypeIds { get; set; }
        public bool IsAgainstProduction { get; set; }
        public DateTime ProtestoStartDate { get; set; }
        public DateTime? ProtestoEndDate { get; set; }
        public List<int> ProtestoPlaceIds { get; set; }
        public int GenderId { get; set; }
        public List<int> InterventionTypeIds { get; set; }
        public List<int> ProtestoCityIds { get; set; }
        public List<int?> ProtestoDistrictIds { get; set; }
        public int? CustodyCount { get; set; }
       
        public int? EmployeeCountInProtesto { get; set; }
        public int? EmployeeCountInProtestoId { get; set; }
        
        public string Note { get; set; }
        public Protesto ToEntity()
        {
            var protesto = new Protesto
            {
                Id = Id,
                ResistanceId = ResistanceId,
                ProtestoEmployeeCountId = EmployeeCountInProtestoId,
                EmployeeCountNumber = EmployeeCountInProtesto,
                StartDate = ProtestoStartDate,
                EndDate = ProtestoEndDate,
                ProtestoProtestoTypes = new List<ProtestoProtestoType>(),
                ProtestoProtestoPlaces = new List<ProtestoProtestoPlace>(),
                ProtestoInterventionTypes = new List<ProtestoInterventionType>(),
                Cities = new List<ProtestoCity>(),
                Districts = new List<ProtestoDistrict>(),
                GenderId = GenderId,
                CustodyCount = CustodyCount,
                Note = Note
                

            };
            if(ProtestoPlaceIds!=null)
                ProtestoPlaceIds.ForEach(c=> protesto.ProtestoProtestoPlaces.Add(new ProtestoProtestoPlace{ ProtestoId = Id, ProtestoPlaceId = c }));
            if(ProtestoTypeIds!=null)
                ProtestoTypeIds.ForEach(c=> protesto.ProtestoProtestoTypes.Add(new ProtestoProtestoType{ ProtestoId = Id, ProtestoTypeId = c }));
            if(InterventionTypeIds!=null)
                InterventionTypeIds.ForEach(c=> protesto.ProtestoInterventionTypes.Add(new ProtestoInterventionType{ ProtestoId = Id, InterventionTypeId = c}));
            if(ProtestoCityIds!=null)
                ProtestoCityIds.ForEach(c=> protesto.Cities.Add(new ProtestoCity{ ProtestoId = Id, CityId = c }));
            if(ProtestoDistrictIds!=null)
                ProtestoDistrictIds.ForEach(c=> {if(c.HasValue) protesto.Districts.Add(new ProtestoDistrict{ ProtestoId = Id, DistrictId = c.Value });});
            return protesto;
        }
    }
}