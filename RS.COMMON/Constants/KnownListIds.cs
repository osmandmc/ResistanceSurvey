namespace RS.COMMON.Constants
{
    /// <summary>
    /// List values the code relies on by id. They exist in production and its copies; a fresh database built from the
    /// migrations has no list values, so code using these ids must not assume the rows exist. Renaming them in Tanımlar is
    /// fine; their meaning is fixed.
    /// </summary>
    public static class KnownListIds
    {
        /// <summary>CorporationType "Sendika": corporations of this type are unions (the union field, "Sendika Var mı").</summary>
        public const int TradeUnionCorporationType = 1;

        /// <summary>InterventionType for "no intervention": choosing it means no custody count is needed.</summary>
        public const int NoInterventionType = 7;
    }
}
