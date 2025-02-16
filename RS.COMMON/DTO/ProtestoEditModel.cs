using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using RS.COMMON.Entities;
using RS.COMMON.Entities.LookupEntity;

namespace RS.COMMON.DTO
{
    public class ProtestoEditModel
    {
        public string UserName { get; set; }
        public int ProtestoId { get; set; }
        public int ResistanceId { get; set; }
        public List<ProtestoType> ProtestoTypeIds { get; set; }
        public bool IsAgainstProduction { get; set; }
        [DataType(DataType.Date)] 
        public DateTime ProtestoStartDate { get; set; }
        [DataType(DataType.Date)] 
        public DateTime? ProtestoEndDate { get; set; }
        public List<ProtestoPlace> ProtestoPlaceIds { get; set; }
        public int GenderId { get; set; }
        public List<InterventionType> InterventionTypeIds { get; set; }
        public List<ProtestoCity> ProtestoCityIds { get; set; }
        public List<ProtestoDistrict> ProtestoDistrictIds { get; set; }
        public List<ProtestoLocationModel> Locations { get; set; }
        public int? CustodyCount { get; set; }
       
        public int? EmployeeCountInProtesto { get; set; }
        public int? EmployeeCountInProtestoId { get; set; }
        public string ResistanceName { get; set; }
        public string Note { get; set; }
        public string SimpleProtestoDescription { get; set; }
        public int StrikeDuration { get; set; }
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
                SimpleProtestoDescription = SimpleProtestoDescription,
                StrikeDuration = StrikeDuration,
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
          
          
            if (Locations != null)
                Locations.Where(s=>!s.Deleted)
                    .ToList()
                    .ForEach(c =>
                    {
                        protesto.Locations.Add(new ProtestoLocation
                        {
                            ProtestoId = ProtestoId, 
                            CityId = c.CityId.Value, 
                            DistrictId = c.DistrictId, 
                            Place = c.Place
                        });
                    });
            return protesto;
        }
    }
}