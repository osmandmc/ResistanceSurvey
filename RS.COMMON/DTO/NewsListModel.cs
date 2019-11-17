using System;
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
    
}