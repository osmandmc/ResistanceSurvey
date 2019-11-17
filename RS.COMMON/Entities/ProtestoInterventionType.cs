using RS.COMMON.Entities.LookupEntity;

namespace RS.COMMON.Entities
{
    public class ProtestoInterventionType
    {
        public int Id { get; set; }
        public int ProtestoId { get; set; }
        public int InterventionTypeId { get; set; }

        public Protesto Protesto { get; set; }
        public InterventionType InterventionType { get; set; }
    }
}