using RS.COMMON.Entities;

namespace RS.COMMON.Entities
{
    public class ResistanceNews
    {
        public int Id { get; set; }
        public int ResistanceId { get; set; }
        public int NewsId { get; set; }

        public Resistance Resistance { get; set; }
        public News News { get; set; }
    }
}