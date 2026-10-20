namespace RS.COMMON.Entities.LookupEntity
{
    public class ResistanceReason : LookupEntity, IApprovable
    {
        public bool Approved { get; set; }
    }
}