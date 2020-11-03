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
        public static PagedResult<T, U> ToPagedFilteredResult<T, U>(this IQueryable<T> queryable, U filter) 
            where T : class 
            where U : FilterModel
        {
            var rowCount = queryable.Count();
            var queryResult = queryable
                .AsNoTracking()
                .Skip((filter.PageNumber -1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();
            return new PagedResult<T, U> ( queryResult, filter, rowCount );
        }
    }
}
