namespace RS.COMMON.Entities.LookupEntity
{
    public class District : LookupEntity
    {
        public int CityId { get; set; }
        
        public City City { get; set; }
    }
}