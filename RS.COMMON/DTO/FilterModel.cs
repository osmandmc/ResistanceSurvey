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
            PageSize = 10;
            PageNumber = 1;
            TotalCount = 0;
        }
        public void SetFilter(bool first, bool prev, int pageNumber, bool next, bool last)
        {
            if (first)
            {
                PageNumber = 1;
            }
            else if (last)
            {
                PageNumber = PageCount;
            }
            else if (prev)
            {
                PageNumber = Math.Max(1, PageNumber - 1);
            }
            else if (next)
            {
                PageNumber = Math.Min(PageCount, PageNumber + 1);
            }
            else
            {
                if (pageNumber >= 1 && pageNumber <= PageCount)
                {
                    PageNumber = pageNumber;
                }
            }
        }
    }
}