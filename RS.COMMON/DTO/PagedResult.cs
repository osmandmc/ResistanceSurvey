using System;
using System.Collections.Generic;

namespace RS.COMMON.DTO
{
    public class PagedResult<T, U> where U: FilterModel
    {
        public PagedResult(IEnumerable<T> results, U filter, int rowCount)
        {
            Filter = filter;
            RowCount = rowCount;
            Results = results;
        }
        public IEnumerable<T> Results { get; set; }
        public U Filter { get; set; }
        public double PageCount => (double)RowCount / Filter.PageSize;
        public int RowCount { get; set; }
        public int From => (Filter.PageNumber - 1) * Filter.PageSize + 1;
        public int To  => Math.Min(RowCount, Filter.PageNumber * Filter.PageSize);
    }

}