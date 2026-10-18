using System;
using System.Globalization;

namespace RS.MVC.Models
{
    /// <summary>
    /// Settings for the news pipeline (direnistakip-pipeline).
    /// News dated in or after the cut-over month comes from the pipeline; older news stays in the local News table.
    /// </summary>
    public class PipelineSettings
    {
        public const string SectionName = "Pipeline";

        /// <summary>First month served by the pipeline, as "yyyy-MM" (e.g. "2026-11"). Empty means the pipeline is off.</summary>
        public string CutoverMonth { get; set; }

        public bool IsValid() =>
            string.IsNullOrWhiteSpace(CutoverMonth) || TryParseCutover(out _);

        /// <summary>True when news dated <paramref name="date"/> is handled by the pipeline.</summary>
        public bool UsesPipeline(DateTime date) =>
            TryParseCutover(out var cutover) && date >= cutover;

        private bool TryParseCutover(out DateTime cutover) =>
            DateTime.TryParseExact(CutoverMonth, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out cutover);
    }
}
