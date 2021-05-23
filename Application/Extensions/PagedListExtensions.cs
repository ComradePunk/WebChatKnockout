using Application.Filters;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Extensions
{
    public static class PagedListExtensions
    {
        public static async Task<PagedList<T>> ToPagedListAsync<T>(this IQueryable<T> query, int? pageNumber, int? pageSize)
        {
            var result = new PagedList<T> { TotalCount = await query.CountAsync() };

            if (!pageNumber.HasValue)
                pageNumber = 1;

            if (!pageSize.HasValue)
                pageSize = 20;

            result.PageNumber = pageNumber.Value;
            result.PageSize = pageSize.Value;
            result.Items = await query.Skip((result.PageNumber - 1) * result.PageSize).Take(result.PageSize).ToListAsync();

            return result;
        }
    }
}
