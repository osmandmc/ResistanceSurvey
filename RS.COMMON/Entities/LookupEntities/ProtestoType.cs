namespace RS.COMMON.Entities.LookupEntity
{
    public class ProtestoType : LookupEntity, IApprovable
    {
        public bool AgainstProduction { get; set; }
        public bool Approved { get; set; }
        public bool Simple { get; set; }
    }
}