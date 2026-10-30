using System.Collections.Generic;

namespace RS.COMMON.Entities.LookupEntity
{
    /// <summary>
    /// An organization: a union, a confederation, a platform, a chamber … One table for all (EF table per hierarchy, R34):
    /// the column Kind says which class a row is, and it follows the type: "Sendika" is a <see cref="TradeUnion"/>,
    /// "Konfederasyon" a <see cref="Confederation"/>, any other type (or none) a plain organization.
    /// </summary>
    public class Corporation : LookupEntity, IApprovable
    {
        public bool Approved { get; set; }
        public int? CorporationTypeId { get; set; }
        public CorporationType CorporationType { get; set; }
    }

    /// <summary>A union; its confederation is optional (independent unions have none). Names keep their prefix ("DİSK Genel-İş").</summary>
    public class TradeUnion : Corporation
    {
        public int? ConfederationId { get; set; }
        public Confederation Confederation { get; set; }
    }

    /// <summary>A confederation of unions (DİSK, TÜRK-İŞ, HAK-İŞ, KESK, Memur-Sen, Türkiye Kamu-Sen, Birleşik Kamu-İş …), kept in Tanımlar.</summary>
    public class Confederation : Corporation
    {
        public List<TradeUnion> Unions { get; set; }
    }

    /// <summary>The Kind column of the organization table.</summary>
    public enum OrganizationKind
    {
        Organization = 0,
        TradeUnion = 1,
        Confederation = 2,
    }
}
