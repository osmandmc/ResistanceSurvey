using System;
using RS.COMMON.Constants;

namespace RS.COMMON.Entities
{
    public class News
    {
        public int Id { get; set; }
        public string Header { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; } 
        public string Link { get; set; }
        public Enums.Status Status { get; set; }
        
    }
}