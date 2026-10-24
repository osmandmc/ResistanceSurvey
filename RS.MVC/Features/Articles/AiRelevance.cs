using System.Collections.Generic;
using RS.MVC.Models;

namespace RS.MVC.Features.Articles
{
    /// <summary>
    /// The pipeline's AI verdict on an article (its Relevance stage, pipeline B9): Verdict is "Relevant", "NotRelevant"
    /// or "Uncertain"; Reason is the AI's one-sentence explanation. Null when the article was not assessed.
    /// </summary>
    public record AiRelevance(string Verdict, string Reason)
    {
        public const string StageName = "Relevance";

        public static AiRelevance From(IReadOnlyDictionary<string, PipelineStageResult> stages) =>
            stages != null && stages.TryGetValue(StageName, out var stage) && stage.Status == "Succeeded" && !string.IsNullOrEmpty(stage.Answer)
                ? new AiRelevance(stage.Answer, stage.Reason)
                : null;
    }
}
