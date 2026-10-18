using System;
using System.Collections.Generic;
using System.Globalization;

namespace RS.MVC.Models
{
    /// <summary>
    /// Settings for the news pipeline (direnistakip-pipeline).
    /// News of a pipeline source dated in or after the cut-over month comes from the pipeline;
    /// everything else stays in the local News table.
    /// </summary>
    public class PipelineSettings
    {
        public const string SectionName = "Pipeline";

        /// <summary>First month served by the pipeline, as "yyyy-MM" (e.g. "2026-11"). Empty means the pipeline is off.</summary>
        public string CutoverMonth { get; set; }

        /// <summary>Pipeline Function App root, e.g. "https://func-xyz.azurewebsites.net".</summary>
        public string BaseUrl { get; set; }

        /// <summary>Function key for the pipeline API. Keep it out of appsettings: use user-secrets or an environment variable.</summary>
        public string FunctionKey { get; set; }

        /// <summary>App source number → pipeline source id, for the sources handled by the pipeline (e.g. 1 → "evrensel").</summary>
        public Dictionary<int, string> Sources { get; set; } = new();

        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(CutoverMonth))
                return true;
            return TryParseCutover(out _)
                && Uri.TryCreate(BaseUrl, UriKind.Absolute, out _)
                && !string.IsNullOrWhiteSpace(FunctionKey);
        }

        /// <summary>True when news of <paramref name="source"/> dated <paramref name="date"/> is handled by the pipeline.</summary>
        public bool UsesPipeline(int source, DateTime date) =>
            Sources.ContainsKey(source) && TryParseCutover(out var cutover) && date >= cutover;

        private bool TryParseCutover(out DateTime cutover) =>
            DateTime.TryParseExact(CutoverMonth, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out cutover);
    }
}
