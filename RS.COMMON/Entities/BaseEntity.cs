using System;
using System.Collections.Generic;
using System.Text;

namespace RS.COMMON.Entities
{
    public class AuditEntity
    {
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string Creator { get; set; }
        public string Updater { get; set; }
    }
}
