using Microsoft.EntityFrameworkCore;
using SaraTouch.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Data.Extensions
{
    public static class QueryableExtensions
    {
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
    this IQueryable<T> query, int page, int pageSize, CancellationToken ct = default)
        {
            var totalCount = await query.CountAsync(ct);

            // Skip ו-Take רצים בתוך ה-SQL (LIMIT/OFFSET), לא בזיכרון
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<T>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
    }
}
