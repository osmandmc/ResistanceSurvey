using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RS.COMMON.Entities;
using RS.EF;
using RS.MVC.Models;

namespace RS.MVC.Features.Review
{
    /// <summary>
    /// Keeps an AI answer the researcher checked next to the decision on the article (R19c): what the answer
    /// suggests and what was saved, both as JSON, so they can be compared field by field later. Called inside the
    /// decision's transaction; nothing is kept when no answer was checked.
    /// </summary>
    public class AiSuggestionRecorder(RSDBContext db, CheckAiAnswerHandler check)
    {
        /// <summary>Answers pasted from the researcher's own AI chat.</summary>
        public const string PastedOrigin = "pasted";

        private static readonly JsonSerializerOptions Json = JsonSerializerOptions.Web;

        /// <summary>Adds the record to the context (the caller saves). <paramref name="saved"/> is what was saved, or null.</summary>
        public async Task AddAsync(News news, AiUse ai, AiDecision decision, int? resistanceId, object saved, DismissKind? dismissKind, string userName)
        {
            if (string.IsNullOrWhiteSpace(ai?.Answer))
                return;
            // The answer is checked again here: the lists as they are now, and nothing taken from the browser but the text.
            var answer = await check.Handle(new CheckAiAnswerCommand(ai.Answer));
            if (answer.Error != null)
                return;
            if (news.Id != 0 && await db.AiSuggestion.AnyAsync(s => s.NewsId == news.Id))
                return;

            db.AiSuggestion.Add(new AiSuggestion
            {
                News = news,
                Decision = decision,
                DismissKind = decision == AiDecision.Dismissed ? dismissKind : null,
                ResistanceId = resistanceId,
                Origin = PastedOrigin,
                PromptVersion = Trim(ai.PromptVersion, 20),
                Answer = ai.Answer,
                Relevant = answer.Relevant,
                Applied = ai.Applied && answer.Relevant,
                Suggested = answer.Relevant ? JsonSerializer.Serialize(new { answer.Case, answer.Protest }, Json) : null,
                Saved = saved == null ? null : JsonSerializer.Serialize(saved, Json),
                CreatedBy = Trim(userName, 50),
                CreatedAt = DateTime.UtcNow,
            });
        }

        /// <summary><see cref="AddAsync"/> and save.</summary>
        public async Task SaveAsync(News news, AiUse ai, AiDecision decision, int? resistanceId, object saved, string userName)
        {
            await AddAsync(news, ai, decision, resistanceId, saved, null, userName);
            await db.SaveChangesAsync();
        }

        private static string Trim(string text, int length) =>
            string.IsNullOrWhiteSpace(text) ? null : text.Trim().Length <= length ? text.Trim() : text.Trim()[..length];
    }
}
