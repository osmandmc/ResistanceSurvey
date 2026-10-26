using System.Collections.Generic;
using RS.MVC.Models;

namespace RS.MVC.Features.Articles
{
    /// <summary>
    /// The pipeline's AI verdict on an article (its Relevance stage, pipeline B9): Verdict is "Relevant", "NotRelevant"
    /// or "Uncertain"; Reason is the AI's one-sentence explanation; Version the prompt and model that judged it
    /// ("relevance-v3.1 · gemini-…"). Null when the article was not assessed.
    /// </summary>
    public record AiRelevance(string Verdict, string Reason, string Version = null)
    {
        public const string StageName = "Relevance";

        public static AiRelevance From(IReadOnlyDictionary<string, PipelineStageResult> stages) =>
            stages != null && stages.TryGetValue(StageName, out var stage) && stage.Status == "Succeeded" && !string.IsNullOrEmpty(stage.Answer)
                ? new AiRelevance(stage.Answer, stage.Reason, stage.Version)
                : null;

        /// <summary>
        /// The AI draft's one-line summary ("company · protest type · city · date"): the Extract stage leaves "Draft" with
        /// the summary as its reason (pipeline B12). Null without a draft.
        /// </summary>
        public static string DraftSummary(IReadOnlyDictionary<string, PipelineStageResult> stages) =>
            stages != null && stages.TryGetValue("Extracted", out var stage) && stage.Status == "Succeeded" && stage.Answer == "Draft"
                ? stage.Reason ?? "Taslak hazır"
                : null;
    }
}
