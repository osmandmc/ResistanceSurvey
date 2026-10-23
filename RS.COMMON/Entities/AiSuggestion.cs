using System;

namespace RS.COMMON.Entities
{
    /// <summary>
    /// "AI ile doldur" (R19c): an AI answer checked for an article, kept beside what the researcher decided, so
    /// suggested and saved values can be compared field by field. One per article, removed with its News row.
    /// </summary>
    public class AiSuggestion
    {
        public int Id { get; set; }

        public int NewsId { get; set; }
        public News News { get; set; }

        public AiDecision Decision { get; set; }

        /// <summary>Why it was dismissed (only for <see cref="AiDecision.Dismissed"/>).</summary>
        public DismissKind? DismissKind { get; set; }

        /// <summary>The case the article went to; null when dismissed.</summary>
        public int? ResistanceId { get; set; }
        public Resistance Resistance { get; set; }

        /// <summary>"pasted" (copied from the researcher's AI chat); later the pipeline's model (R11).</summary>
        public string Origin { get; set; }
        public string PromptVersion { get; set; }

        /// <summary>The answer as pasted.</summary>
        public string Answer { get; set; }

        /// <summary>The answer's own verdict.</summary>
        public bool Relevant { get; set; }

        /// <summary>The researcher used "Forma uygula".</summary>
        public bool Applied { get; set; }

        /// <summary>The form values the answer gives (JSON, ids), as checked when the decision was saved.</summary>
        public string Suggested { get; set; }

        /// <summary>What was saved (JSON, the request's case and protest), copied now: later edits of the case do not count.</summary>
        public string Saved { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public enum AiDecision
    {
        NewCase = 1,
        AddProtest = 2,
        LinkOnly = 3,
        Dismissed = 4,
    }

    /// <summary>Why an article was dismissed ("İlgisiz olarak işaretle").</summary>
    public enum DismissKind
    {
        /// <summary>Emek hareketiyle ilgisiz.</summary>
        NotLabour = 1,

        /// <summary>Zaten kayıtlı / tekrar haber.</summary>
        AlreadyRecorded = 2,

        /// <summary>Kapsam dışı: labour news the team does not record.</summary>
        OutOfScope = 3,

        /// <summary>Diğer.</summary>
        Other = 4,
    }
}
