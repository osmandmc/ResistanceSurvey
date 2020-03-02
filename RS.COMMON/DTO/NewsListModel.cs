using System;
using RS.COMMON.Constants;
using static RS.COMMON.Constants.Enums;

namespace RS.COMMON.DTO
{
    public class NewsListModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public Status Status { get; set; }
        public string Link { get; set; }
    }
    public class NewsItem{
        public int Id { get; set; }
        public string Header { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; } 
        public string Link { get; set; }
        public bool Added { get; set; }
    }
}