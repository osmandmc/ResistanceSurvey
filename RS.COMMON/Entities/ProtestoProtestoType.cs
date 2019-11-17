using RS.COMMON.Entities.LookupEntity;

namespace RS.COMMON.Entities
{
    public class ProtestoProtestoType
    {
        public int Id { get; set; }
        public int ProtestoId { get; set; }
        public int ProtestoTypeId { get; set; }

        public Protesto Protesto { get; set; }
        public ProtestoType ProtestoType { get; set; }
    }
}