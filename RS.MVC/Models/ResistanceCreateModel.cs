using System;
using System.Collections.Generic;

namespace ResistanceSurvey.Models
{
    public class ResistanceCreateModel
    {
        public int CompanyId { get; set; }
        public DateTime StartDate { get; set; }
        public List<KeyValuePair<int, string>> Companies { get; set; }
         
    }
}