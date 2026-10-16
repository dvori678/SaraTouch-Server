using AutoMapper;
using SaraTouch.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Service.Mapping
{
    public static class PagedResultExtensions
    {
        public static PagedResult<TDest> MapItems<TSource, TDest>(this PagedResult<TSource> page, IMapper mapper) => new()
        {
            Items = mapper.Map<List<TDest>>(page.Items),
            Page = page.Page,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount
        };
    }
}
