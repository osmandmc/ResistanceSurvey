using Microsoft.EntityFrameworkCore;
using RS.COMMON.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RS.EF
{
    public static class Extensions
    {
        public static PagedResult<T> ToPagedFilteredResult<T>(this IQueryable<T> queryable, FilterModel filter) where T : class
        {
            var rowCount = queryable.Count();
            var pageCount = (double)rowCount / filter.PageSize;
            var queryResult = queryable
                .AsNoTracking()
                .Skip((filter.PageNumber -1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();
            return new PagedResult<T> { Results = queryResult, CurrentPage = filter.PageNumber, PageCount = (int)Math.Ceiling(pageCount), PageSize = filter.PageSize, RowCount = rowCount };
        }
    }
}
