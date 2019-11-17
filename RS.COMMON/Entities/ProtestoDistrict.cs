using RS.COMMON.Entities.LookupEntity;

namespace RS.COMMON.Entities
{
    public class ProtestoDistrict
    {
        public int Id { get; set; }
        public int ProtestoId { get; set; }
        public int DistrictId { get; set; }

        public District District { get; set; }
        public Protesto Protesto { get; set; }
    }
}