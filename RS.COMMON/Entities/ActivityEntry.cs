using System;

namespace RS.COMMON.Entities
{
    /// <summary>
    /// One line of the activity log (F4b): who did what, when. Written in the same save as the change itself, by every
    /// action that changes data (inbox decisions, cases and protests, Tanımlar, accounts, uploads). It records that
    /// something changed, not the old and new values.
    /// </summary>
    public class ActivityEntry
    {
        public long Id { get; set; }
        public DateTime At { get; set; }

        /// <summary>The signed-in username (null for none, e.g. a person setting their password is named by the account).</summary>
        public string Username { get; set; }

        /// <summary>What was done, e.g. "case.update" (the codes are in RS.MVC Features/Activity).</summary>
        public string Action { get; set; }

        /// <summary>The case it belongs to, for the case page's Geçmiş.</summary>
        public int? CaseId { get; set; }

        /// <summary>The pipeline article id, when an article was handled.</summary>
        public string ArticleId { get; set; }

        /// <summary>What else it concerns: a Tanımlar kind ("companies" …) or "user", with its id.</summary>
        public string Subject { get; set; }
        public int? SubjectId { get; set; }

        /// <summary>A short line for people (Turkish): names, counts.</summary>
        public string Summary { get; set; }
    }
}
