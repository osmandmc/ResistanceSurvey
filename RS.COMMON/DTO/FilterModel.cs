using System;

namespace RS.COMMON.DTO
{
    public class FilterModel
    {
        public int TotalCount { get; set; }
        public int PageCount { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }

        public FilterModel()
        {
            PageSize = 20;
            PageNumber = 1;
            TotalCount = 0;
        }
    }
}