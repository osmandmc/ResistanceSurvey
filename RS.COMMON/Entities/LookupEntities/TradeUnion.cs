namespace RS.COMMON.Entities.LookupEntity
{
    public class TradeUnion : LookupEntity
    {
        public int? TradeUnionConfederationId { get; set; }
        public TradeUnionConfederation TradeUnionConfederation { get; set; }
        public bool Approved { get; set; }
    }
}