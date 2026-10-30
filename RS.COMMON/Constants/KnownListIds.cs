namespace RS.COMMON.Constants
{
    /// <summary>
    /// List values the code relies on by id. They exist in production and its copies; a fresh database built from the
    /// migrations has no list values, so code using these ids must not assume the rows exist. Renaming them in Tanımlar is
    /// fine; their meaning is fixed.
    /// </summary>
    public static class KnownListIds
    {
        /// <summary>CorporationType "Sendika": organizations of this type are unions (TradeUnion, R34).</summary>
        public const int TradeUnionCorporationType = 1;

        /// <summary>CorporationType "Konfederasyon" (added by migration Unions, R34): organizations of this type are confederations.</summary>
        public const int ConfederationCorporationType = 6;

        /// <summary>InterventionType for "no intervention": choosing it means no custody count is needed.</summary>
        public const int NoInterventionType = 7;

        /// <summary>
        /// The three case categories (R31): fixed, each with its own fields. İşletme/Kurum: a workplace's struggle (employer,
        /// main company, union, counts, result). Dayanışma: an action supporting another struggle (organizers, the supported
        /// case). Genel: an action organizations called without a workplace (organizers; the issue is the case reason).
        /// </summary>
        public const int WorkplaceCategory = 1;
        public const int SolidarityCategory = 2;
        public const int GeneralCategory = 3;
    }
}
