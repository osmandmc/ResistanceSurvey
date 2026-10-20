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

        /// <summary>Pipeline Function App root, e.g. "https://func-xyz.azurewebsites.net".</summary>
        public string BaseUrl { get; set; }

        /// <summary>Function key for the pipeline API. Keep it out of appsettings: use user-secrets or an environment variable.</summary>
        public string FunctionKey { get; set; }

        /// <summary>App source number → pipeline source id, for the sources handled by the pipeline (e.g. 1 → "evrensel").</summary>
        public Dictionary<int, string> Sources { get; set; } = new();

        /// <summary>When the pipeline address is set, it must be a full URL and come with its key.</summary>
        public bool IsValid() =>
            string.IsNullOrWhiteSpace(BaseUrl)
            || (Uri.TryCreate(BaseUrl, UriKind.Absolute, out _) && !string.IsNullOrWhiteSpace(FunctionKey));

        /// <summary>True when the news of this app source goes to the pipeline (all news does since R12b; the source must be mapped).</summary>
        public bool UsesPipeline(int source) => Sources.ContainsKey(source);
    }
}
