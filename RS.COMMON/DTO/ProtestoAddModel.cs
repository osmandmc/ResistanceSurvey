using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using RS.COMMON.Entities;

namespace RS.COMMON.DTO
{
    public class ProtestoAddModel
    {
        public int ResistanceId { get; set; }
        public List<int> ProtestoTypeIds { get; set; }
        public bool IsAgainstProduction { get; set; }
        [DataType(DataType.Date)] 
        public DateTime ProtestoStartDate { get; set; }
        [DataType(DataType.Date)]         
        public DateTime? ProtestoEndDate { get; set; }
        public List<int> ProtestoPlaceIds { get; set; }
        public int GenderId { get; set; }
        public List<int> InterventionTypeIds { get; set; }
        public int? CustodyCount { get; set; }
        public int? EmployeeCountInProtesto { get; set; }
        public int? EmployeeCountInProtestoId { get; set; }
        public bool DevelopRight { get; set; }
        public List<ProtestoLocationModel> Locations { get; set; }
        public List<int> ProtestoCityIds { get; set; }
        public List<int?> ProtestoDistrictIds { get; set; }
        public string UserName { get; set; }
        public string Note { get; set; }
        public string SimpleProtestoDescription { get; set; }
        public int StrikeDuration { get; set; }
        
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
                Cities = new List<ProtestoCity>(),
                Districts = new List<ProtestoDistrict>(),
                Locations = new List<ProtestoLocation>(),
                GenderId = GenderId,
                CustodyCount = CustodyCount,
                Note = Note,
                StrikeDuration = StrikeDuration
            };
            if(ProtestoPlaceIds!=null)
                ProtestoPlaceIds.ForEach(c=> protesto.ProtestoProtestoPlaces.Add(new ProtestoProtestoPlace{ ProtestoPlaceId = c }));
            if(ProtestoTypeIds!=null)
                ProtestoTypeIds.ForEach(c=> protesto.ProtestoProtestoTypes.Add(new ProtestoProtestoType{ ProtestoTypeId = c }));
            if(InterventionTypeIds!=null)
                InterventionTypeIds.ForEach(c=> protesto.ProtestoInterventionTypes.Add(new ProtestoInterventionType{ InterventionTypeId = c}));
           
             if(ProtestoCityIds!=null)
                ProtestoCityIds.ForEach(c=> protesto.Cities.Add(new ProtestoCity{ CityId = c }));
            if(ProtestoDistrictIds!=null)
                ProtestoDistrictIds.ForEach(c=> {if(c.HasValue) protesto.Districts.Add(new ProtestoDistrict{ DistrictId = c.Value });});
            if (Locations != null)
                Locations.Where(s=>!s.Deleted).ToList().ForEach(c => { protesto.Locations.Add(new ProtestoLocation { CityId = c.CityId.Value, DistrictId = c.DistrictId.Value, Place = c.Place }); });
            return protesto;
        }
    }
}