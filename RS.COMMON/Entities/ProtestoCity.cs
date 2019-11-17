using RS.COMMON.Entities.LookupEntity;

namespace RS.COMMON.Entities
{
    public class ProtestoCity
    {
        public int Id { get; set; }
        public int ProtestoId { get; set; }
        public int CityId { get; set; }

        public City City { get; set; }
        public Protesto Protesto { get; set; }
    }
}