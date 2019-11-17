using RS.COMMON.Entities.LookupEntity;

namespace RS.COMMON.Entities
{
    public class ResistanceEmploymentType
    {
        public int Id { get; set; }
        public int ResistanceId { get; set; }
        public int EmploymentTypeId { get; set; }

        public Resistance Resistance { get; set; }
        public EmploymentType EmploymentType { get; set; }
    }
}