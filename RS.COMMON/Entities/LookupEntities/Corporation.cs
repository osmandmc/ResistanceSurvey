namespace RS.COMMON.Entities.LookupEntity
{
    public class Corporation : LookupEntity, IApprovable
    {
        public bool Approved { get; set; }
        public int? CorporationTypeId { get; set; }
        public CorporationType CorporationType { get; set; }
    }
}