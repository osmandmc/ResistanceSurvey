using RS.COMMON.Entities.LookupEntity;

namespace RS.COMMON.Entities
{
    public class ProtestoProtestoPlace
    {
        public int Id { get; set; }
        public int ProtestoId { get; set; }
        public int ProtestoPlaceId { get; set; }

        public Protesto Protesto { get; set; }
        public ProtestoPlace ProtestoPlace { get; set; }
    }
}