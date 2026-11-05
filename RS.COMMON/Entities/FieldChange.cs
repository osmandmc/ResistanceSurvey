using System;

namespace RS.COMMON.Entities
{
    /// <summary>
    /// One changed field (R39): which record, which field, the old and the new value, who, when and how (a person, an
    /// import …). Written on every save by the app's save hook, in the same save as the change. Lists (reasons,
    /// organizations, protest types …) give one row per value added (OldValue empty) or removed (NewValue empty).
    /// Values are kept as stored (ids, enum names, dates as yyyy-MM-dd); the screens turn them into names.
    /// </summary>
    public class FieldChange
    {
        public long Id { get; set; }
        public DateTime At { get; set; }

        /// <summary>The signed-in username (null when nobody is signed in, e.g. a background job).</summary>
        public string Username { get; set; }

        /// <summary>How the change was made: "person", "import", "migration" or "system" (RS.MVC Features/History).</summary>
        public string Source { get; set; }

        /// <summary>The activity line of the same request, when it wrote one: its Geçmiş line shows these changes.</summary>
        public long? ActivityEntryId { get; set; }
        public ActivityEntry ActivityEntry { get; set; }

        /// <summary>The kind of record ("Resistance", "Protesto", "Company", "Workplace", "Corporation") and its id.</summary>
        public string Entity { get; set; }
        public int RecordId { get; set; }

        /// <summary>The case it belongs to (a case's own fields, or one of its protests), for the case page.</summary>
        public int? CaseId { get; set; }

        /// <summary>The property name (e.g. "CompanyId"), or the list's name (e.g. "Reasons").</summary>
        public string Field { get; set; }

        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}
