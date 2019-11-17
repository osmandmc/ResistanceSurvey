using RS.COMMON.Entities.LookupEntity;

namespace RS.COMMON.Entities
{
    public class ResistanceResistanceReason
    {
        public int Id { get; set; }
        public int ResistanceId { get; set; }
        public int ResistanceReasonId { get; set; }

        public Resistance Resistance { get; set; }
        public ResistanceReason ResistanceReason { get; set; }
    }
}