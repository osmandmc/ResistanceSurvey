using System;
using System.Collections.Generic;
using RS.COMMON.Entities;
using static RS.COMMON.Constants.Enums;

namespace RS.MVC.Models
{
    // Shapes of the new app's review screen (/app/review/{articleId}). Ids only; nullable where the form may be empty,
    // so the server can say which required field is missing.

    public static class ReviewAction
    {
        /// <summary>A new resistance with its first protest.</summary>
        public const string NewCase = "newCase";

        /// <summary>A new protest on an existing resistance.</summary>
        public const string AddProtest = "addProtest";

        /// <summary>Only attach the article to an existing resistance.</summary>
        public const string LinkOnly = "linkOnly";
    }

    public class ReviewSaveRequest
    {
        public string Action { get; set; }

        /// <summary>The existing resistance, for AddProtest and LinkOnly.</summary>
        public int? ResistanceId { get; set; }

        /// <summary>For NewCase.</summary>
        public CaseInput Case { get; set; }

        /// <summary>For NewCase and AddProtest.</summary>
        public ProtestInput Protest { get; set; }

        /// <summary>The AI answer checked for this article ("AI ile doldur"), if any; kept with the decision (R19c).</summary>
        public AiUse Ai { get; set; }
    }

    /// <summary>An AI answer the researcher checked on the review screen.</summary>
    public class AiUse
    {
        /// <summary>The answer as pasted.</summary>
        public string Answer { get; set; }

        public string PromptVersion { get; set; }

        /// <summary>"Forma uygula" was used.</summary>
        public bool Applied { get; set; }

        /// <summary>What made the answer: null when pasted from the researcher's AI chat, the pipeline's for a draft (R11).</summary>
        public string Origin { get; set; }
    }

    /// <summary>The "Vaka (Direniş)" part of the form.</summary>
    public class CaseInput
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int? CategoryId { get; set; }
        public ResistanceTargetType? TargetType { get; set; }
        public int? CompanyId { get; set; }
        public bool IsOutsource { get; set; }
        public int? MainCompanyId { get; set; }
        public int? TargetCorporationId { get; set; }
        public List<int> ReasonIds { get; set; } = [];

        /// <summary>Reasons typed by the researcher; matched to existing ones by name, otherwise created.</summary>
        public List<string> NewReasonNames { get; set; } = [];

        public bool? DevelopRight { get; set; }
        public int? EmployeeCount { get; set; }
        public int? EmployeeCountRangeId { get; set; }
        public List<int> CorporationIds { get; set; } = [];

        /// <summary>Corporations typed by the researcher; matched to existing ones by name, otherwise created.</summary>
        public List<string> NewCorporationNames { get; set; } = [];

        public int? TradeUnionAuthorityId { get; set; }
        public int? TradeUnionId { get; set; }
        public List<int> EmploymentTypeIds { get; set; } = [];
        public int? FiredEmployeeCount { get; set; }
        public ResistanceResult? Result { get; set; }
    }

    /// <summary>The "Eylem" part of the form.</summary>
    public class ProtestInput
    {
        public List<int> ProtestTypeIds { get; set; } = [];
        public int? StrikeDurationSeconds { get; set; }
        public string SimpleDescription { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<int> PlaceIds { get; set; } = [];
        public int? GenderId { get; set; }
        public List<LocationInput> Locations { get; set; } = [];
        public int? EmployeeCount { get; set; }
        public int? EmployeeCountRangeId { get; set; }
        public List<int> InterventionTypeIds { get; set; } = [];
        public int? CustodyCount { get; set; }
        public string Note { get; set; }
    }

    public class LocationInput
    {
        public int? CityId { get; set; }
        public int? DistrictId { get; set; }
        public string Place { get; set; }
    }

    /// <summary>The article shown beside the form: its full text from the pipeline.</summary>
    /// <param name="Ai">The pipeline's AI verdict on the article; null when not assessed.</param>
    public record ReviewArticle(string ArticleId, string Title, string Url, string SourceName, DateTimeOffset? PublishedAt, string Body,
        RS.MVC.Features.Articles.AiRelevance Ai = null);

    /// <summary>
    /// An existing resistance to add a protest to, or to link the article to. Ongoing: its latest protest has no end
    /// date. Cities: where its protests took place.
    /// </summary>
    public record ResistanceOption(
        int Id,
        string Name,
        string CompanyName,
        DateTime? LastProtestDate,
        int ProtestCount,
        bool Ongoing,
        IReadOnlyList<string> Cities);
}
