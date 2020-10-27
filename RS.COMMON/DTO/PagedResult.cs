using System;
using System.Collections.Generic;

namespace RS.COMMON.DTO
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Results { get; set; }
        public int CurrentPage { get; set; }
        public int PageCount { get; set; }
        public int PageSize { get; set; }
        public int RowCount { get; set; }
        public int From => (CurrentPage - 1) * PageSize + 1;
        public int To  => Math.Min(RowCount, CurrentPage * PageSize);
    }

}