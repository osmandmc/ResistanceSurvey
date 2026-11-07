using System;
using System.Collections.Generic;
using System.Text;

namespace RS.COMMON.Entities
{
    /// <summary>Who created and last changed the record, and when; its version guards against lost edits (IVersioned).</summary>
    public class AuditEntity : IVersioned
    {
        public int Version { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string Creator { get; set; }
        public string Updater { get; set; }
    }
}
