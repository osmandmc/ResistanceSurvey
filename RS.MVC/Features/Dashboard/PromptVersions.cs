using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.COMMON.Entities;
using RS.EF;
using RS.MVC.Applications;
using RS.MVC.Features.Activity;
using RS.MVC.Features.Review;
using RS.MVC.Models;
using static RS.COMMON.Constants.Enums;

namespace RS.MVC.Features.Dashboard
{
    /// <summary>The prompts' names the pipeline knows.</summary>
    public static class PromptNames
    {
        public const string Relevance = "relevance";
        public const string Extraction = "extraction";
        public const string Builtin = "builtin";

        public static bool IsKnown(string name) => name is Relevance or Extraction;
    }

    public record CreatePromptVersionCommand(string Name, string Text, string Note, string UserName);

    /// <summary>The version saved (not active yet), or why the pipeline refused it.</summary>
    public record PromptVersionResult(PipelinePromptVersion Version, string Error);

    /// <summary>Saves an edited prompt as a new version in the pipeline (R28); it runs only after "Etkinleştir".</summary>
    public class CreatePromptVersionHandler(IPipelineClient pipeline, RSDBContext db)
    {
        public async Task<PromptVersionResult> Handle(CreatePromptVersionCommand command)
        {
            var (version, error) = await pipeline.CreatePromptVersionAsync(command.Name, command.Text ?? "", command.Note, command.UserName);
            if (version == null)
                return new PromptVersionResult(null, error);
            db.LogActivity(Actions.PromptCreate, $"{PromptLabel(command.Name)}: {version.Version}" + (string.IsNullOrWhiteSpace(command.Note) ? "" : $" ({command.Note.Trim()})"));
            await db.SaveChangesAsync();
            return new PromptVersionResult(version, null);
        }

        internal static string PromptLabel(string name) => name == PromptNames.Relevance ? "İlgi kontrolü" : "Taslak istemi";
    }

    public record ActivatePromptCommand(string Name, string Version, string UserName);

    /// <summary>The pipeline runs the version from now on (within a minute); "builtin" goes back to the repository's prompt.</summary>
    public class ActivatePromptHandler(IPipelineClient pipeline, RSDBContext db)
    {
        public async Task<bool> Handle(ActivatePromptCommand command)
        {
            if (!await pipeline.ActivatePromptAsync(command.Name, command.Version, command.UserName))
                return false;
            var label = command.Version == PromptNames.Builtin ? "yerleşik sürüm" : command.Version;
            db.LogActivity(Actions.PromptActivate, $"{CreatePromptVersionHandler.PromptLabel(command.Name)}: {label}");
            await db.SaveChangesAsync();
            return true;
        }
    }

    /// <summary>
    /// One article of a try. Decision: what researchers did ("linked", "dismissed", or for the extraction prompt "newCase"
    /// / "addProtest"). Relevance: Current and Tried are verdicts, Expected says which verdicts agree with the decision
    /// (null: the decision says nothing about relevance, e.g. dismissed as already recorded). Extraction: Current and
    /// Tried are the drafts' summaries; Kept… the share of the AI's values researchers kept.
    /// </summary>
    public record PromptTryRow(
        string ArticleId,
        string Title,
        string Decision,
        string Current,
        string Tried,
        string TriedReason,
        string Error,
        bool? CurrentAgrees = null,
        bool? TriedAgrees = null,
        int? KeptCurrent = null,
        int? KeptTried = null);

    /// <summary>
    /// The comparison: per article, and in total (relevance: verdicts that agree with the decision; extraction: per field
    /// the share kept with the running prompt and with the tried one).
    /// </summary>
    public record PromptTry(
        string Name,
        string Version,
        string Model,
        IReadOnlyList<PromptTryRow> Rows,
        int Compared,
        int CurrentAgrees,
        int TriedAgrees,
        IReadOnlyList<PromptTryField> Fields,
        string Message);

    /// <summary>One field: of the values the AI gave, the share researchers kept (null: the AI gave none).</summary>
    public record PromptTryField(string Field, string Label, int? KeptCurrent, int? KeptTried, int GivenCurrent, int GivenTried);

    /// <summary>
    /// "Dene" (R28): runs a prompt version on up to 10 recent articles researchers decided, and compares its answers with
    /// the running prompt's and with the decisions. Relevance: the latest linked and dismissed articles, half each.
    /// Extraction: the latest new cases and added protests saved with an AI answer, whose saved values are the reference.
    /// Spends one AI request per article; nothing is saved.
    /// </summary>
    public class TryPromptHandler(IPipelineClient pipeline, RSDBContext db, CheckAiAnswerHandler check)
    {
        public const int Articles = 10;

        public async Task<PromptTry> Handle(string name, string version, string userName)
        {
            var result = name == PromptNames.Relevance ? await RelevanceAsync(version) : await ExtractionAsync(version);
            if (result != null)
            {
                db.LogActivity(Actions.PromptTry, $"{CreatePromptVersionHandler.PromptLabel(name)}: {result.Version}, {result.Rows.Count} haber");
                await db.SaveChangesAsync();
            }
            return result;
        }

        private async Task<PromptTry> RelevanceAsync(string version)
        {
            var decided = db.News.AsNoTracking().Where(n => n.ArticleId != null);
            var linked = await decided.Where(n => n.Status == Status.Linked).OrderByDescending(n => n.Id).Take(Articles / 2)
                .Select(n => new { n.Id, n.ArticleId, Linked = true }).ToListAsync();
            var dismissed = await decided.Where(n => n.Status == Status.Passive).OrderByDescending(n => n.Id).Take(Articles - linked.Count)
                .Select(n => new { n.Id, n.ArticleId, Linked = false }).ToListAsync();
            var articles = linked.Concat(dismissed).ToList();
            if (articles.Count == 0)
                return Empty(PromptNames.Relevance, version, "Henüz karar verilmiş haber yok; denemek için önce birkaç haberi bağlayın ya da ilgisiz işaretleyin.");

            // Why an article was dismissed is known when an AI answer was kept with the decision (R19c).
            var newsIds = articles.Select(a => a.Id).ToList();
            var kinds = await db.AiSuggestion.Where(s => newsIds.Contains(s.NewsId) && s.DismissKind != null)
                .ToDictionaryAsync(s => s.NewsId, s => s.DismissKind!.Value);

            var tried = await pipeline.TryPromptVersionAsync(PromptNames.Relevance, version, articles.Select(a => a.ArticleId).ToList());
            if (tried == null)
                return null;

            var rows = new List<PromptTryRow>();
            foreach (var r in tried.Results)
            {
                var article = articles.First(a => a.ArticleId == r.ArticleId);
                var kind = kinds.TryGetValue(article.Id, out var k) ? k : (DismissKind?)null;
                // Linked: relevant or uncertain is right. Dismissed as not labour (or without a reason): not relevant is
                // right. Dismissed as already recorded, out of scope or other: the verdict cannot be judged by it.
                bool? Agrees(string verdict) => verdict == null ? null
                    : article.Linked ? verdict is "Relevant" or "Uncertain"
                    : kind is null or DismissKind.NotLabour ? verdict == "NotRelevant"
                    : null;
                var decision = article.Linked ? "linked" : kind is { } d ? $"dismissed:{d}" : "dismissed";
                rows.Add(new PromptTryRow(r.ArticleId, r.Title, decision, r.CurrentAnswer, r.Answer, r.Reason, r.Error,
                    Agrees(r.CurrentAnswer), r.Error == null ? Agrees(r.Answer) : null));
            }
            var compared = rows.Where(r => r.TriedAgrees != null && r.CurrentAgrees != null).ToList();
            return new PromptTry(PromptNames.Relevance, tried.Version, tried.Model, rows, compared.Count,
                compared.Count(r => r.CurrentAgrees == true), compared.Count(r => r.TriedAgrees == true), [], null);
        }

        private async Task<PromptTry> ExtractionAsync(string version)
        {
            var answers = await db.AiSuggestion.AsNoTracking()
                .Where(s => s.Saved != null && (s.Decision == AiDecision.NewCase || s.Decision == AiDecision.AddProtest) && s.News.ArticleId != null)
                .OrderByDescending(s => s.Id).Take(Articles)
                .Select(s => new { s.News.ArticleId, s.Decision, s.Saved })
                .ToListAsync();
            if (answers.Count == 0)
                return Empty(PromptNames.Extraction, version,
                    "Karşılaştırılacak kayıt yok: denemek için AI yanıtıyla (“AI ile doldur” ya da işleme hattının taslağı) kaydedilmiş vakalar gerekiyor.");

            var tried = await pipeline.TryPromptVersionAsync(PromptNames.Extraction, version, answers.Select(a => a.ArticleId).ToList());
            if (tried == null)
                return null;

            var names = await AiReportHandler.Names.LoadAsync(db);
            var current = new AiReportHandler.FieldTally(names);
            var draft = new AiReportHandler.FieldTally(names);
            var rows = new List<PromptTryRow>();
            foreach (var r in tried.Results)
            {
                var answer = answers.First(a => a.ArticleId == r.ArticleId);
                var saved = AiReportHandler.Read<AiReportHandler.Saved>(answer.Saved);
                var keptCurrent = await CompareAsync(current, r.CurrentAnswer, saved, answer.Decision);
                var keptTried = r.Error == null ? await CompareAsync(draft, r.Answer, saved, answer.Decision) : null;
                rows.Add(new PromptTryRow(r.ArticleId, r.Title, answer.Decision == AiDecision.NewCase ? "newCase" : "addProtest",
                    r.CurrentReason, r.Reason, null, r.Error, KeptCurrent: keptCurrent, KeptTried: keptTried));
            }

            var currentFields = current.Result().ToDictionary(f => f.Field);
            var triedFields = draft.Result().ToDictionary(f => f.Field);
            var fields = currentFields.Values.Concat(triedFields.Values).Select(f => (f.Field, f.Label)).Distinct()
                .Select(f =>
                {
                    var c = currentFields.GetValueOrDefault(f.Field);
                    var t = triedFields.GetValueOrDefault(f.Field);
                    return new PromptTryField(f.Field, f.Label, Share(c), Share(t), Given(c), Given(t));
                })
                .ToList();
            var compared = rows.Where(r => r.KeptCurrent != null && r.KeptTried != null).ToList();
            return new PromptTry(PromptNames.Extraction, tried.Version, tried.Model, rows, compared.Count,
                compared.Count(r => r.KeptCurrent >= r.KeptTried), compared.Count(r => r.KeptTried >= r.KeptCurrent), fields, null);
        }

        /// <summary>Adds the answer to the tally; the share of its values kept in this article, or null when there is nothing to compare.</summary>
        private async Task<int?> CompareAsync(AiReportHandler.FieldTally tally, string answerJson, AiReportHandler.Saved saved, AiDecision decision)
        {
            if (string.IsNullOrWhiteSpace(answerJson) || saved == null)
                return null;
            var answer = await check.Handle(new CheckAiAnswerCommand(answerJson));
            if (answer.Error != null || !answer.Relevant)
                return 0; // an answer without a labour action keeps nothing
            var one = new AiReportHandler.FieldTally(null, tally);
            if (decision == AiDecision.NewCase && answer.Case != null && saved.Case != null)
                one.AddCase(answer.Case, saved.Case);
            if (answer.Protest != null && saved.Protest != null)
                one.AddProtest(answer.Protest, saved.Protest);
            var (kept, given) = one.Totals();
            return given == 0 ? null : (int)Math.Round(100.0 * kept / given);
        }

        private static int Given(AiReportField f) => f == null ? 0 : f.Kept + f.Changed + f.Removed;

        private static int? Share(AiReportField f) => Given(f) == 0 ? null : (int)Math.Round(100.0 * f.Kept / Given(f));

        private static PromptTry Empty(string name, string version, string message) => new(name, version, null, [], 0, 0, 0, [], message);
    }
}
