using RS.COMMON.Entities.LookupEntity;

namespace RS.COMMON.Entities
{
    public class ResistanceCorporation
    {
        public int Id { get; set; }
        public int ResistanceId { get; set; }
        public int CorporationId { get; set; }

        public Resistance Resistance { get; set; }
        public Corporation Corporation { get; set; }
    }
}