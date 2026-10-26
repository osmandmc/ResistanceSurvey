using System;
using System.Globalization;
using RS.COMMON.Entities;
using RS.EF;

namespace RS.MVC.Features.Activity
{
    /// <summary>The activity log's action codes (F4b). The screens show them in Turkish; the part before the dot is the area.</summary>
    public static class Actions
    {
        public const string ArticleNewCase = "article.new-case";
        public const string ArticleAddProtest = "article.add-protest";
        public const string ArticleLink = "article.link";
        public const string ArticleDismiss = "article.dismiss";

        public const string CaseCreate = "case.create";
        public const string CaseUpdate = "case.update";
        public const string CaseDelete = "case.delete";
        public const string CaseAttachArticle = "case.attach-article";
        public const string CaseDetachArticle = "case.detach-article";
        public const string ProtestAdd = "protest.add";
        public const string ProtestUpdate = "protest.update";
        public const string ProtestDelete = "protest.delete";

        public const string DefinitionUpdate = "definition.update";
        public const string DefinitionApprove = "definition.approve";
        public const string DefinitionUnapprove = "definition.unapprove";
        public const string DefinitionDelete = "definition.delete";
        public const string DefinitionMerge = "definition.merge";
        public const string DefinitionNotDuplicate = "definition.not-duplicate";
        public const string CompanyCreate = "definition.company-create";
        public const string CompanyLinkAdd = "definition.company-link-add";
        public const string CompanyLinkRemove = "definition.company-link-remove";

        public const string UserCreate = "user.create";
        public const string UserLink = "user.link";
        public const string UserRole = "user.role";
        public const string UserDisable = "user.disable";
        public const string UserEnable = "user.enable";
        public const string UserPassword = "user.password";

        public const string UploadExcel = "upload.excel";
        public const string UploadLinks = "upload.links";
        public const string UploadLocalNews = "upload.local-news";

        public const string PromptCreate = "prompt.create";
        public const string PromptTry = "prompt.try";
        public const string PromptActivate = "prompt.activate";
    }

    public static class ActivityLog
    {
        public const int MaxSummary = 500;

        private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");

        /// <summary>
        /// Adds a line to the log; the caller's next SaveChanges writes it, in the same transaction as the change. The
        /// person is the signed-in user (<see cref="ActivityInterceptor"/>); <paramref name="username"/> only when nobody
        /// is signed in yet (a person setting their password).
        /// </summary>
        public static ActivityEntry LogActivity(this RSDBContext db, string action, string summary,
            int? caseId = null, string articleId = null, string subject = null, int? subjectId = null, string username = null)
        {
            var entry = new ActivityEntry
            {
                At = DateTime.UtcNow,
                Username = username,
                Action = action,
                CaseId = caseId,
                ArticleId = articleId,
                Subject = subject,
                SubjectId = subjectId,
                Summary = summary is { Length: > MaxSummary } ? summary[..(MaxSummary - 1)] + "…" : summary,
            };
            db.ActivityEntry.Add(entry);
            return entry;
        }

        /// <summary>"12.10.2026", for protests in summaries.</summary>
        public static string Day(DateTime date) => date.ToString("dd.MM.yyyy", Turkish);

        /// <summary>“name”, or "" for none.</summary>
        public static string Quoted(string name) => string.IsNullOrWhiteSpace(name) ? "" : $"“{name.Trim()}”";
    }
}
