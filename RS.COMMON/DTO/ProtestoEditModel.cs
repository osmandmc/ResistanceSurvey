using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using RS.COMMON.Entities;

namespace RS.COMMON.DTO
{
    public class ProtestoEditModel
    {
        public int ProtestoId { get; set; }
        public int ResistanceId { get; set; }
        public List<string> ProtestoTypeIds { get; set; }
        public bool IsAgainstProduction { get; set; }
        [DataType(DataType.Date)] 
        public DateTime ProtestoStartDate { get; set; }
        [DataType(DataType.Date)] 
        public DateTime? ProtestoEndDate { get; set; }
        public List<string> ProtestoPlaceIds { get; set; }
        public int GenderId { get; set; }
        public List<int> InterventionTypeIds { get; set; }
        public List<int> ProtestoCityIds { get; set; }
        public List<int?> ProtestoDistrictIds { get; set; }
        public List<ProtestoLocationModel> Locations { get; set; }
        public int? CustodyCount { get; set; }
       
        public int? EmployeeCountInProtesto { get; set; }
        public int? EmployeeCountInProtestoId { get; set; }
        public string ResistanceName { get; set; }
        public string Note { get; set; }
        public string Updater { get; set; }
        public Protesto ToEntity()
        {
            var protesto = new Protesto
            {
                Id = ProtestoId,
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
                Locations = new List<ProtestoLocation>(),
                GenderId = GenderId,
                CustodyCount = CustodyCount,
                Note = Note,
                UpdateDate = DateTime.Now,
                Updater = Updater
            };
           
          
            if(InterventionTypeIds!=null)
                InterventionTypeIds.ForEach(c=> protesto.ProtestoInterventionTypes.Add(new ProtestoInterventionType{ ProtestoId = ProtestoId, InterventionTypeId = c}));
            if(ProtestoCityIds!=null)
                ProtestoCityIds.ForEach(c=> protesto.Cities.Add(new ProtestoCity{ ProtestoId = ProtestoId, CityId = c }));
            if(ProtestoDistrictIds!=null)
                ProtestoDistrictIds.ForEach(c=> {if(c.HasValue) protesto.Districts.Add(new ProtestoDistrict{ ProtestoId = ProtestoId, DistrictId = c.Value });});
            if (Locations!= null)
                Locations.Where(s=>!s.Deleted).ToList().ForEach(c => { protesto.Locations.Add(new ProtestoLocation { ProtestoId = ProtestoId, CityId = c.CityId.Value, DistrictId = c.DistrictId, Place = c.Place }); });
            return protesto;
        }
    }
}