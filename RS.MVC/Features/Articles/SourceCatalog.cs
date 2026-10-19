using System.Collections.Generic;

namespace RS.MVC.Features.Articles
{
    /// <summary>Pipeline source ids → names shown to users.</summary>
    public static class SourceCatalog
    {
        private static readonly Dictionary<string, string> Names = new()
        {
            ["evrensel"] = "Evrensel",
            ["kizilbayrak"] = "Kızılbayrak",
            ["sendika"] = "Sendika.org",
            ["birgun"] = "BirGün",
        };

        public static string Name(string sourceId) =>
            sourceId != null && Names.TryGetValue(sourceId, out var name) ? name : sourceId;
    }
}
